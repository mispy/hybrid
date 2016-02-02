using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipDesigner : MonoBehaviour {
    public Blockform designShip;
    public Blueprint cursor;
    public bool isDragging = false;
    public bool isRemoving = false;
    public bool isMirroring = false;
    public IntVector2 dragOrigin;
    public IntVector2 mousePos;
    Block cursorBlock;
    bool isCursorValid = false;
    bool isMirrorValid = false;
    Facing currentFacing = Facing.up;

    public ConsoleLinker consoleLinker { get; private set; }

    public Dictionary<Block, LineRenderer> blockingLines = new Dictionary<Block, LineRenderer>();

    public void Awake() {
        consoleLinker = GetComponentsInChildren<ConsoleLinker>(includeInactive: true).First();
    }

    void AddBlockingLine(Block block) {
        var worldPos = designShip.BlockToWorldPos(block.pos);
        var dir = designShip.transform.TransformDirection((Vector2)block.facing);
        var line = Annotation.DrawLine(worldPos, worldPos + (Vector2)dir*10, Color.green, 0.05f);
        line.transform.SetParent(cursor.transform);
        blockingLines[block] = line;
    }

    public void OnEnable() {        
        if (Game.playerShip == null) {
            gameObject.SetActive(false);
            return;
        }

        Game.shipControl.gameObject.SetActive(false);

        cursor = Pool.For("Blueprint").Attach<Blueprint>(transform);
        cursor.name = "Cursor";
        cursor.blocks = Pool.For("BlockMap").Attach<BlockMap>(cursor.transform);

        foreach (var renderer in cursor.tiles.MeshRenderers) {
            renderer.sortingLayerName = "UI";
        }

        //Game.main.debugText.text = "Designing Ship";
        //Game.main.debugText.color = Color.green;        
        designShip = Game.playerShip;

        cursor.transform.SetParent(designShip.transform);
        cursor.transform.position = designShip.transform.position + new Vector3(0, 0, -1);
        cursor.transform.rotation = designShip.transform.rotation;

        foreach (var blockObj in designShip.GetComponentsInChildren<BlockType>()) {
            blockObj.spriteRenderer.enabled = false;
        }
        
        foreach (var block in designShip.blocks.frontBlockers) {
            AddBlockingLine(block);
        }

 //       Game.Pause();

        InputEvent.For(KeyCode.C).Bind(this, () => consoleLinker.gameObject.SetActive(true));
        InputEvent.For(KeyCode.M).Bind(this, () => isMirroring = !isMirroring);
        InputEvent.For(KeyCode.LeftBracket).Bind(this, () => currentFacing = currentFacing.RotatedLeft());
        InputEvent.For(KeyCode.RightBracket).Bind(this, () => currentFacing = currentFacing.RotatedRight());

        Game.playerShip.blocks.OnBlockAdded += OnBlockAdded;
        Game.playerShip.blocks.OnBlockRemoved += OnBlockRemoved;
    }

    public void OnBlockAdded(Block block) {
        if (block.type.canBlockFront)
            AddBlockingLine(block);
    }

    public void OnBlockRemoved(Block block) {
        if (block.type.canBlockFront) {
            Destroy(blockingLines[block]);
        }
    }
    
    public void OnDisable() {
        if (cursor != null)
            Pool.Recycle(cursor.gameObject);
        Game.shipControl.gameObject.SetActive(true);
        Game.playerShip.blocks.OnBlockAdded -= OnBlockAdded;
        Game.playerShip.blocks.OnBlockRemoved -= OnBlockRemoved;
        //Game.main.debugText.text = "";
        //Game.main.debugText.color = Color.white;
   //     Game.Unpause();
    }

    Block FindAdjoiningBlock(Vector2 worldPos, IntVector2 blockPos) {
        var neighborBlocks = new List<Block>();
        foreach (var pos in IntVector2.Neighbors(blockPos)) {
            var block = designShip.blocks[pos, BlockLayer.Base];
            if (block != null) {
                neighborBlocks.Add(block);
                continue;
            }

            block = cursor.blocks[pos, BlockLayer.Base];
            if (block != null) neighborBlocks.Add(block);
        }    

        if (neighborBlocks.Count == 0)
            return null;

        return neighborBlocks.OrderBy((block) => Vector2.Distance(worldPos, designShip.BlockToWorldPos(block.pos))).First();
    }

    Facing FacingFromAdjoining(IntVector2 pos, Block adjoiningBlock) {
        if (adjoiningBlock == null) return Facing.up;
        return (Facing)((pos - adjoiningBlock.pos).normalized);
    }

    bool CanFitInto(Block cursorBlock, IntVector2 pos) {
        var baseBlock = designShip.blocks[pos, BlockLayer.Base];

        if (cursorBlock.layer == BlockLayer.Base)
            return true;
        
        // Top layer blocks go on top of floor, or sometimes inside walls
        if (cursorBlock.layer == BlockLayer.Top && baseBlock != null) {
            if (baseBlock.Is("Floor")) 
                return true;
            if (baseBlock.Is("Wall") && cursorBlock.type.canFitInsideWall)
                return true;
        }

        return false;
    }

    Block AdjoiningBlock(Block block, IntVector2 pos) {
        var adjoining = Util.AdjoiningBlock(block, pos, designShip.blocks);
        if (adjoining != null) return adjoining;

        return Util.AdjoiningBlock(block, pos, cursor.blocks);
    }


    bool IsValidPlacement(Block block, IntVector2 desiredPos) {
        foreach (var pos in IntVector2.Rectangle(desiredPos, block.Width, block.Height)) {
            if (!CanFitInto(block, pos))
                return false;

            if (IsBlockedByLOF(pos))
                return false;

            if (block.type.canBlockFront && !HasLineOfFire(pos, block.facing))
                return false;
        }

        if (!IsValidAttachment(block, desiredPos))
            return false;
        
        return true;
    }



    bool IsValidAttachment(Block block, IntVector2 desiredPos) {
        if (block.type.canRotate) {
            // For things like turrets, we want to ensure the entire base of the block lines up with
            // an appropriate attaching surface
            foreach (var pos in IntVector2.SideOfRectangle(desiredPos, block.Width, block.Height, -block.facing)) {
                var offset = (IntVector2)(-block.facing);
                var baseBlock = designShip.blocks[pos+offset, BlockLayer.Base];
                if (baseBlock == null || baseBlock.CollisionLayer != Block.wallLayer)
                    return false;
            }


            return true;
        } else {
            // For normal blocks, they simply need to be neighboring another base block to attach
            foreach (var pos in IntVector2.NeighborsOfRectangle(desiredPos, block.Width, block.Height)) {
                if (designShip.blocks[pos, BlockLayer.Base] != null || cursor.blocks[pos, BlockLayer.Base] != null)
                    return true;
            }            

            return false;
        }
    }


    bool IsBlockedByLOF(IntVector2 desiredPos) {

        foreach (var blocker in designShip.blocks.frontBlockers) {
            if (desiredPos.x == blocker.x && blocker.facing == Facing.up && desiredPos.y > blocker.y)
                return true;
            if (desiredPos.x == blocker.x && blocker.facing == Facing.down && desiredPos.y < blocker.y)
                return true;
            if (desiredPos.y == blocker.y && blocker.facing == Facing.right && desiredPos.x > blocker.x)
                return true;
            if (desiredPos.y == blocker.y && blocker.facing == Facing.left && desiredPos.x < blocker.x)
                return true;
        }

        return false;
    }

    bool HasLineOfFire(IntVector2 blockPos, Facing dir) {
        var pos = blockPos;

        while (!designShip.blocks.IsOutsideBounds(pos)) {
            pos = pos + (IntVector2)dir;

            if (designShip.blocks[pos, BlockLayer.Base] != null)
                return false;
        }

        return true;
    }

    void FinishDrag() {
        isDragging = false;

        foreach (var block in cursor.blocks.allBlocks) {
            if (Game.inventory[block.type] == 0 && !Game.debugMode)
                continue;
            
            Game.inventory[block.type] -= 1;
            designShip.blocks[block.pos,  block.layer] = new Block(block);
        }
       
        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (block.pos != IntVector2.zero)
                cursor.blocks[block.pos, block.layer] = null;
        }
    }

    IntVector2 MirrorPosition(IntVector2 pos) {
        var cx = Mathf.RoundToInt(designShip.centerOfMass.x / Tile.worldSize);
        return new IntVector2(cx + (cx - pos.x), pos.y);
    }

    void FinishRemoving() {
        isRemoving = false;

        foreach (var block in cursor.blocks.allBlocks) {
            var removedBlock = designShip.blocks[block.pos, block.layer];
            if (removedBlock != null) {
                Game.inventory[removedBlock.type] += 1;
            }
            if (block.layer == BlockLayer.Base) {
                var topBlock = designShip.blocks[block.pos, BlockLayer.Top];
                if (topBlock != null)
                    Game.inventory[topBlock.type] += 1;
            }
            designShip.blocks[block.pos, block.layer] = null;
        }
    }

    void UpdateRemoving() {
        if (!Input.GetMouseButton(1)) {
            FinishRemoving();
            return;
        }
        
        foreach (var renderer in cursor.tiles.MeshRenderers)
            renderer.material.color = Color.red;
        
        var rect = new IntRect(dragOrigin, mousePos);
        var mirrorOrigin = MirrorPosition(dragOrigin);
        var mirrorRect = new IntRect(mirrorOrigin, MirrorPosition(mousePos));

        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (!rect.Contains(block.pos) && (!isMirroring || !mirrorRect.Contains(block.pos)))
                cursor.blocks[block.pos, block.layer] = null;
        }
        
        for (var i = rect.minX; i <= rect.maxX; i++) {
            for (var j = rect.minY; j <= rect.maxY; j++) {
                var pos = new IntVector2(i, j);
                var currentBlock = designShip.blocks[pos, BlockLayer.Base];

                if (currentBlock != null)
                    cursor.blocks[currentBlock.pos, currentBlock.layer] = new Block(currentBlock);
            }
        }

        if (isMirroring) {
            for (var i = mirrorRect.minX; i <= mirrorRect.maxX; i++) {
                for (var j = mirrorRect.minY; j <= mirrorRect.maxY; j++) {
                    var pos = new IntVector2(i, j);
                    var currentBlock = designShip.blocks[pos, BlockLayer.Base];
                    
                    if (currentBlock != null)
                        cursor.blocks[currentBlock.pos, currentBlock.layer] = new Block(currentBlock);
                }
            }
        }
    }

    void UpdateDrag() {        
        if (!Input.GetMouseButton(0)) {
            FinishDrag();
            return;
        }


        var rect = new IntRect(dragOrigin, mousePos);
        var mirrorOrigin = MirrorPosition(dragOrigin);
        var mirrorRect = new IntRect(mirrorOrigin, MirrorPosition(mousePos));
        

        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (!rect.Contains(block.pos) && (!isMirroring || !mirrorRect.Contains(block.pos)))
                cursor.blocks[block.pos, block.layer] = null;
            else if (!IsValidPlacement(block, block.pos))
                cursor.blocks[block.pos, block.layer] = null;   
        }

        if (isCursorValid) {
            for (var i = rect.minX; i <= rect.maxX; i++) {
                for (var j = rect.minY; j <= rect.maxY; j++) {
                    var pos = new IntVector2(i, j);
                    var newBlock = new Block(cursorBlock);

                    if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(newBlock, pos))
                        cursor.blocks[pos, newBlock.layer] = newBlock;
                }
            }
        }

        if (isMirroring && isMirrorValid) {
            for (var i = mirrorRect.minX; i <= mirrorRect.maxX; i++) {
                for (var j = mirrorRect.minY; j <= mirrorRect.maxY; j++) {
                    var pos = new IntVector2(i, j);
                    var newBlock = new Block(cursorBlock);

                    if (newBlock.facing == Facing.right)
                        newBlock.facing = Facing.left;
                    else if (newBlock.facing == Facing.left)
                        newBlock.facing = Facing.right;

                    if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(newBlock, pos))
                        cursor.blocks[pos, newBlock.layer] = newBlock;
                }
            }
        }
    }

    void Update() {
        mousePos = designShip.WorldToBlockPos(Game.mousePos);

        if (isDragging) {
            UpdateDrag();
            return;
        }

        if (isRemoving) {
            UpdateRemoving();
            return;
        }

        foreach (var block in cursor.blocks.allBlocks.ToList())
            cursor.blocks[block.pos, block.layer] = null;

        var selectedType = Game.blockSelector.selectedType;
        cursorBlock = new Block(selectedType);
        if (cursorBlock.type.canRotate)
            cursorBlock.facing = currentFacing;
        //var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, mousePos);                    
        //cursorBlock.facing = FacingFromAdjoining(mousePos, adjoiningBlock);

        cursor.blocks[mousePos, selectedType.blockLayer] = cursorBlock;
        isCursorValid = IsValidPlacement(cursorBlock, mousePos);

        if (isMirroring) {
            var mirrorPos = MirrorPosition(mousePos);
            var mirrorBlock = new Block(selectedType);
            if (cursorBlock.facing == Facing.left)
                mirrorBlock.facing = Facing.right;
            else if (cursorBlock.facing == Facing.right)
                mirrorBlock.facing = Facing.left;
            cursor.blocks[mirrorPos, selectedType.blockLayer] = mirrorBlock;

            isMirrorValid = IsValidPlacement(cursorBlock, mirrorPos);
        } else {
            isMirrorValid = true;
        }

        if (isCursorValid) {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.green;
        } else {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.red;
        }


        if (Input.GetKeyDown(KeyCode.F1)) {
            gameObject.SetActive(false);
        }

       // Debug.LogFormat("{0} {1} {2} {3}", cursor.blocks.minX, cursor.blocks.maxX, cursor.blocks.minY, cursor.blocks.maxY);

        //if (EventSystem.current.IsPointerOverGameObject())
        //    return;

        if (Input.GetMouseButton(0)) {
            if (!isCursorValid && !isMirrorValid) return;

            dragOrigin = mousePos;
            isDragging = true;
            foreach (var pos in cursor.blocks.FilledPositions)
                cursor.blocks[pos, selectedType.blockLayer] = null;
            UpdateDrag();
        }

        if (Input.GetMouseButton(1)) {
            dragOrigin = mousePos;
            isRemoving = true;
            foreach (var pos in cursor.blocks.FilledPositions)
                cursor.blocks[pos, selectedType.blockLayer] = null;
            UpdateRemoving();
        }
    }
}
