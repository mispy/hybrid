using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipDesigner : MonoBehaviour {
    public Ship designShip;
    public Blueprint cursor;
    public bool isDragging = false;
    public bool isMirroring = true;
    public IntVector2 startDragPos;
    public IntVector2 mousePos;
    BlueprintBlock cursorBlock;

    public void OnEnable() {        
        cursor = Pool.For("Blueprint").Take<Blueprint>();
        cursor.name = "Cursor";
        cursor.blocks = new BlockMap(null);
        cursor.gameObject.SetActive(true);

		cursor.tiles.EnableRendering();
        Game.shipControl.gameObject.SetActive(false);

        //Game.main.debugText.text = "Designing Ship";
        //Game.main.debugText.color = Color.green;        
        SetDesignShip(Game.playerShip);
        cursor.transform.position = designShip.form.transform.position;
        cursor.transform.rotation = designShip.form.transform.rotation;

        Game.Pause();
    }
    
    public void OnDisable() {
        Pool.Recycle(cursor.gameObject);

        if (designShip != null) {
			designShip.form.blueprint.tiles.DisableRendering();
			designShip.form.tiles.EnableRendering();
            foreach (var blockObj in designShip.form.GetComponentsInChildren<BlockType>()) {
                blockObj.renderer.enabled = true;
            }
        }
        Game.shipControl.gameObject.SetActive(true);
        //Game.main.debugText.text = "";
        //Game.main.debugText.color = Color.white;
        Game.Unpause();
    }

    void SetDesignShip(Ship ship) {
        /*if (designShip != null) {
			designShip.form.blueprint.tiles.DisableRendering();
			designShip.form.tiles.EnableRendering();
        }*/

        designShip = ship;
		designShip.form.tiles.DisableRendering();
		designShip.form.blueprint.tiles.EnableRendering();

        foreach (var blockObj in designShip.form.GetComponentsInChildren<BlockType>()) {
            blockObj.renderer.enabled = false;
        }
	}

    Block FindAdjoiningBlock(Vector2 worldPos, IntVector2 blockPos) {
        var neighborBlocks = new List<Block>();
        foreach (var pos in IntVector2.Neighbors(blockPos)) {
            var block = designShip.form.blueprint.blocks[pos, BlockLayer.Base];
            if (block != null) 
                neighborBlocks.Add(block);
        }    

        if (neighborBlocks.Count == 0)
            return null;

        return neighborBlocks.OrderBy((block) => Vector2.Distance(worldPos, designShip.form.BlockToWorldPos(block.pos))).First();
    }

    Facing RotationFromAdjoining(IntVector2 pos, Block adjoiningBlock) {
        if (adjoiningBlock == null) return Facing.up;
        return (Facing)((adjoiningBlock.pos - pos).normalized);
    }

    bool CanFitInto(Block cursorBlock, Block existingBlock) {
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

    BlueprintBlock AdjoiningBlock(IntVector2 pos, IntVector2 dir) {
        var block = cursor.blocks[pos+dir, BlockLayer.Base];
        if (block != null) return (BlueprintBlock)block;
        
        block = designShip.blueprintBlocks[pos+dir, BlockLayer.Base];
        return (BlueprintBlock)block;
    }

    BlueprintBlock AdjoiningBlock(IntVector2 pos, Facing rot) {
        return AdjoiningBlock(pos, (IntVector2)rot);
    }

    bool IsValidPlacement(IntVector2 pos, BlueprintBlock block) {
        var adjoiningBlock = AdjoiningBlock(block.pos, block.facing);
        if (adjoiningBlock == null) {
            var dir = (cursorBlock.pos - pos).normalized;
            adjoiningBlock = AdjoiningBlock(block.pos, dir);
            if (adjoiningBlock == null)
                return false;
            else
                block.facing = (Facing)dir;
        }

        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                foreach (var currentBlock in designShip.form.blueprint.blocks.BlocksAtPos(new IntVector2(pos.x+i, pos.y+j))) {
                    //Debug.LogFormat("{0} {1} {2}", cursorBlock.type.name, block.type.name, CanFitInto(cursorBlock, block));
                    if (!CanFitInto(block, currentBlock))
                        return false;
                }
            }
        }
        
        return true;
    }

    bool IsValidPlacement(BlueprintBlock cursorBlock) {
        return IsValidPlacement(cursorBlock.pos, cursorBlock);
    }

    void FinishDrag() {
        isDragging = false;

        foreach (var block in cursor.blocks.allBlocks) {
            if (!IsValidPlacement((BlueprintBlock)block)) continue;
            designShip.blueprintBlocks[block.pos,  block.layer] = new BlueprintBlock(block);
        }
       
        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (block.pos != IntVector2.zero)
                cursor.blocks[block.pos, block.layer] = null;
        }
    }

    IntVector2 MirrorPosition(IntVector2 pos) {
        var cx = Mathf.RoundToInt(designShip.form.centerOfMass.x / Tile.worldSize);
        return new IntVector2(cx + (cx - pos.x), pos.y);
    }

    void UpdateDrag() {        
        if (!Input.GetMouseButton(0)) {
            FinishDrag();
            return;
        }

        var rect = new IntRect(startDragPos, mousePos);
        var mirrorRect = new IntRect(MirrorPosition(startDragPos), MirrorPosition(mousePos));

        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (!rect.Contains(block.pos) && !mirrorRect.Contains(block.pos))
                cursor.blocks[block.pos, block.layer] = null;
        }

        for (var i = rect.minX; i <= rect.maxX; i++) {
            for (var j = rect.minY; j <= rect.maxY; j++) {
                var pos = new IntVector2(i, j);
                var newBlock = new BlueprintBlock(cursorBlock);

                if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(pos, newBlock))
                    cursor.blocks[pos, newBlock.layer] = newBlock;
            }
        }

        for (var i = mirrorRect.minX; i <= mirrorRect.maxX; i++) {
            for (var j = mirrorRect.minY; j <= mirrorRect.maxY; j++) {
                var pos = new IntVector2(i, j);
                var newBlock = new BlueprintBlock(cursorBlock);

                if (newBlock.facing == Facing.right)
                    newBlock.facing = Facing.left;
                else if (newBlock.facing == Facing.left)
                    newBlock.facing = Facing.right;

                if (cursor.blocks[pos, newBlock.layer] == null && IsValidPlacement(pos, newBlock))
                    cursor.blocks[pos, newBlock.layer] = newBlock;
            }
        }
    }

    void Update() {
        mousePos = designShip.form.WorldToBlockPos(Game.mousePos);

        if (isDragging) {
            UpdateDrag();
            return;
        }

        foreach (var block in cursor.blocks.allBlocks.ToList())
            cursor.blocks[block.pos, block.layer] = null;

        var selectedType = MainUI.blockSelector.selectedType;
        cursorBlock = new BlueprintBlock(selectedType);
        var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, mousePos);                    
        cursorBlock.facing = RotationFromAdjoining(mousePos, adjoiningBlock);

        cursor.blocks[mousePos, selectedType.blockLayer] = cursorBlock;
        var mirrorPos = MirrorPosition(mousePos);
        var mirrorBlock = new BlueprintBlock(selectedType);
        if (cursorBlock.facing == Facing.left)
            mirrorBlock.facing = Facing.right;
        else if (cursorBlock.facing == Facing.right)
            mirrorBlock.facing = Facing.left;
        cursor.blocks[mirrorPos, selectedType.blockLayer] = mirrorBlock;

        var isValid = IsValidPlacement(mousePos, cursorBlock);
        
        if (isValid) {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.green;
        } else {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.red;
        }


        if (Input.GetKeyDown(KeyCode.F1)) {
            gameObject.SetActive(false);
        }

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(0)) {
            startDragPos = mousePos;
            isDragging = true;
        }

        /*if (Input.GetMouseButton(0) && isValid) {            
            //designShip.blueprintBlocks[targetBlockPos, cursorBlock.layer] = new BlueprintBlock(cursorBlock);
            //designShip.blocks[targetBlockPos, cursorBlock.layer] = new Block(cursorBlock);
        } else if (Input.GetMouseButton(1)) {
            designShip.blueprintBlocks.RemoveSurface(targetBlockPos);
            designShip.blocks.RemoveSurface(targetBlockPos);
        }*/
    }
}
