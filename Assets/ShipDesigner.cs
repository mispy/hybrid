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

    public ConsoleLinker consoleLinker { get; private set; }

    public void Awake() {
        consoleLinker = GetComponentsInChildren<ConsoleLinker>(includeInactive: true).First();
    }

    public void OnEnable() {        
        Game.shipControl.gameObject.SetActive(false);

        cursor = Pool.For("Blueprint").Attach<Blueprint>(transform);
        cursor.name = "Cursor";
        cursor.blocks = Pool.For("BlockMap").Attach<BlockMap>(cursor.transform);

        foreach (var renderer in cursor.tiles.MeshRenderers) {
            renderer.sortingLayerName = "UI";
        }

        //Game.main.debugText.text = "Designing Ship";
        //Game.main.debugText.color = Color.green;        
        designShip = Game.localPlayer.crew.maglockShip;

        cursor.transform.SetParent(designShip.transform);
        cursor.transform.position = designShip.transform.position + new Vector3(0, 0, -1);
        cursor.transform.rotation = designShip.transform.rotation;

        foreach (var blockObj in designShip.GetComponentsInChildren<BlockType>()) {
            blockObj.spriteRenderer.enabled = false;
        }
        
        foreach (var block in designShip.blocks.frontBlockers) {
            /*var worldPos = designShip.BlockToWorldPos(block.pos);
            var dir = designShip.transform.TransformDirection((Vector2)block.facing);
            var line = Annotation.DrawLine(worldPos, worldPos + (Vector2)dir*10, Color.green, 0.05f);
            line.transform.SetParent(cursor.transform);*/
        }

 //       Game.Pause();

        InputEvent.For(KeyCode.C).Bind(this, () => consoleLinker.gameObject.SetActive(true));
        InputEvent.For(KeyCode.M).Bind(this, () => isMirroring = !isMirroring);
    }
    
    public void OnDisable() {
        Pool.Recycle(cursor.gameObject);
        Game.shipControl.gameObject.SetActive(true);
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
        var existingBlock = designShip.blocks[pos, BlockLayer.Base];

        // Base layer blocks only go in empty space adjacent to an existing block
        if (cursorBlock.layer == BlockLayer.Base && existingBlock == null)
            return true;
        
        // Top layer blocks go on top of floor, or sometimes inside walls
        if (cursorBlock.layer == BlockLayer.Top && existingBlock != null) {
            if (existingBlock.Is("Floor")) 
                return true;
            if (existingBlock.Is("Wall") && cursorBlock.type.canFitInsideWall)
                return true;
        }

        return false;
    }

    Block AdjoiningBlock(Block block, IntVector2 pos) {
        var adjoining = Util.AdjoiningBlock(block, pos, designShip.blocks);
        if (adjoining != null) return adjoining;

        return Util.AdjoiningBlock(block, pos, cursor.blocks);
    }

    bool IsValidPlacement(IntVector2 desiredPos, Block block) {
        var adjoiningBlock = AdjoiningBlock(block, desiredPos);
        if (adjoiningBlock == null)
            return false;

        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                var pos = new IntVector2(desiredPos.x+i, desiredPos.y+j);

                if (!CanFitInto(block, pos))
                    return false;

                if (PositionBlocked(pos))
                    return false;

                if (block.type.canBlockFront && !HasLineOfFire(pos, block.facing))
                    return false;
            }
        }
        
        return true;
    }

    bool PositionBlocked(IntVector2 pos) {
        foreach (var blocker in designShip.blocks.frontBlockers) {
            if (pos.x == blocker.x && blocker.facing == Facing.up && pos.y > blocker.y)
                return true;
            if (pos.x == blocker.x && blocker.facing == Facing.down && pos.y < blocker.y)
                return true;
            if (pos.y == blocker.y && blocker.facing == Facing.right && pos.x > blocker.x)
                return true;
            if (pos.y == blocker.y && blocker.facing == Facing.left && pos.x < blocker.x)
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

    bool IsValidPlacement(Block cursorBlock) {
        return IsValidPlacement(cursorBlock.pos, cursorBlock);
    }

    void FinishDrag() {
        isDragging = false;

        foreach (var block in cursor.blocks.allBlocks) {
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
            else if (!IsValidPlacement(block))
                cursor.blocks[block.pos, block.layer] = null;   
        }

        if (isCursorValid) {
            for (var i = rect.minX; i <= rect.maxX; i++) {
                for (var j = rect.minY; j <= rect.maxY; j++) {
                    var pos = new IntVector2(i, j);
                    var newBlock = new Block(cursorBlock);

                    if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(pos, newBlock))
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

                    if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(pos, newBlock))
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
        var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, mousePos);                    
        cursorBlock.facing = FacingFromAdjoining(mousePos, adjoiningBlock);

        cursor.blocks[mousePos, selectedType.blockLayer] = cursorBlock;
        isCursorValid = IsValidPlacement(mousePos, cursorBlock);

        if (isMirroring) {
            var mirrorPos = MirrorPosition(mousePos);
            var mirrorBlock = new Block(selectedType);
            if (cursorBlock.facing == Facing.left)
                mirrorBlock.facing = Facing.right;
            else if (cursorBlock.facing == Facing.right)
                mirrorBlock.facing = Facing.left;
            cursor.blocks[mirrorPos, selectedType.blockLayer] = mirrorBlock;

            isMirrorValid = IsValidPlacement(mirrorPos, cursorBlock);
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

        if (EventSystem.current.IsPointerOverGameObject())
            return;

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
