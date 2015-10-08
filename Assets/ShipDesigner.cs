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
    public IntVector2 targetBlockPos;

    BlueprintBlock cursorBlock;
    IntVector2 mousedCursorPos;

    public void OnEnable() {        
        cursor = Pool.For("Blueprint").Take<Blueprint>();
        cursor.name = "Cursor";
        cursor.blocks = new BlockMap(null);
        cursor.blocks[0, 0, BlockLayer.Base] = BlueprintBlock.Make("Wall");
		cursor.gameObject.SetActive(true);

		cursor.tiles.EnableRendering();
        Game.shipControl.gameObject.SetActive(false);

        //Game.main.debugText.text = "Designing Ship";
        //Game.main.debugText.color = Color.green;        
        SetDesignShip(Game.playerShip);
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
        foreach (var bp in IntVector2.Neighbors(blockPos)) {
            var block = designShip.form.blueprint.blocks[bp, BlockLayer.Base];
            if (block != null) 
                neighborBlocks.Add(block);
        }    

        if (neighborBlocks.Count == 0)
            return null;

        return neighborBlocks.OrderBy((block) => Vector2.Distance(worldPos, designShip.form.BlockToWorldPos(block.pos))).First();
    }

    void UpdateRotation(Block cursorBlock, IntVector2 targetBlockPos, Block adjoiningBlock) {
        if (!cursorBlock.type.canRotate || adjoiningBlock == null) return;
        
        Orientation ori;
        if (targetBlockPos.x < adjoiningBlock.pos.x) {
            ori = Orientation.left;
        } else if (targetBlockPos.x > adjoiningBlock.pos.x) {
            ori = Orientation.right;
        } else if (targetBlockPos.y > adjoiningBlock.pos.y) {
            ori = Orientation.up;
        } else {
            ori = Orientation.down;
        }
        
        if (ori != cursorBlock.orientation) {
            var block = new BlueprintBlock(cursorBlock);
            block.orientation = ori;
            cursor.blocks[0, 0, block.layer] = block;
        }
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

    bool IsValidPlacement(IntVector2 targetBlockPos, BlueprintBlock cursorBlock, Block adjoiningBlock) {
        for (var i = 0; i < cursorBlock.Width; i++) {
            for (var j = 0; j < cursorBlock.Height; j++) {
                foreach (var block in designShip.form.blueprint.blocks.BlocksAtPos(new IntVector2(targetBlockPos.x+i, targetBlockPos.y+j))) {
                    //Debug.LogFormat("{0} {1} {2}", cursorBlock.type.name, block.type.name, CanFitInto(cursorBlock, block));
                    if (!CanFitInto(cursorBlock, block))
                        return false;
                }
            }
        }
        
        return true;
    }

    void FinishDrag(bool isValid) {
        isDragging = false;

        if (isValid) {
            foreach (var block in cursor.blocks.allBlocks) {
                designShip.blueprintBlocks[targetBlockPos + block.pos,  block.layer] = new BlueprintBlock(cursor.blocks.Topmost(new IntVector2(0, 0)));
            }
        }
       
        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (block.pos != IntVector2.Zero)
                cursor.blocks[block.pos, cursorBlock.layer] = null;
        }
    }

    void UpdateDrag() {
        
        var isValid = true;
        foreach (var block in cursor.blocks.allBlocks) {
            var shipBlockPos = targetBlockPos + block.pos;
            var adjoiningBlock = designShip.blueprintBlocks[shipBlockPos + (IntVector2)block.orientation, BlockLayer.Base];
            if (adjoiningBlock == null) 
                adjoiningBlock = cursor.blocks[block.pos + (IntVector2)block.orientation, BlockLayer.Base];
            if (!IsValidPlacement(shipBlockPos, cursorBlock, adjoiningBlock))
                isValid = false;
        }
        
        if (isValid) {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.green;
        } else {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.red;
        }

        if (!Input.GetMouseButton(0)) {
            FinishDrag(isValid);
            return;
        }

        var rect = new IntRect(IntVector2.Zero, mousedCursorPos);

        foreach (var block in cursor.blocks.allBlocks.ToList()) {
            if (!rect.Contains(block.pos))
                cursor.blocks[block.pos, cursorBlock.layer] = null;
        }

        for (var i = rect.minX; i <= rect.maxX; i++) {
            for (var j = rect.minY; j <= rect.maxY; j++) {
                if (cursor.blocks[i, j, cursorBlock.layer] == null)
                    cursor.blocks[i, j, cursorBlock.layer] = new BlueprintBlock(cursorBlock);
            }
        }
    }

    void Update() {
        cursorBlock = (BlueprintBlock)cursor.blocks.Topmost(IntVector2.Zero);
        mousedCursorPos = cursor.WorldToBlockPos(Game.mousePos);

        if (isDragging) {
            UpdateDrag();
            return;
        }

        var selectedType = MainUI.blockSelector.selectedType;
        if (cursor.blocks.Topmost(IntVector2.Zero).type != selectedType) {
            cursor.blocks.RemoveSurface(IntVector2.Zero);
            cursor.blocks[IntVector2.Zero, selectedType.blockLayer] = new BlueprintBlock(selectedType);
        }

        cursor.transform.position = Game.mousePos;
        targetBlockPos = designShip.form.WorldToBlockPos(Game.mousePos);

        var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, targetBlockPos);                    
        UpdateRotation(cursorBlock, targetBlockPos, adjoiningBlock);

        var isValid = IsValidPlacement(targetBlockPos, cursorBlock, adjoiningBlock);
        
        if (isValid) {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.green;
        } else {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.red;
        }

        var worldPos = designShip.form.BlockToWorldPos(targetBlockPos);
        cursor.transform.position = new Vector3(worldPos.x, worldPos.y, designShip.form.transform.position.z-1);
        cursor.transform.rotation = designShip.form.transform.rotation;
        
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(KeyCode.F1)) {
            gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0)) {
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
