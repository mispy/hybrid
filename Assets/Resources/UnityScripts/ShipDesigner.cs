using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipDesigner : MonoBehaviour {
    public Ship designShip;
    public Blueprint cursor;

    public void OnEnable() {        
        if (cursor == null) {
            var cursorObj = Pool.For("Blueprint").TakeObject();
            cursorObj.name = "Cursor";
            cursor = cursorObj.GetComponent<Blueprint>();
            cursor.blocks = new BlockMap();
            cursor.blocks[0, 0, BlockLayer.Base] = BlueprintBlock.Make<Wall>();
            cursorObj.SetActive(true);
        } else {
            cursor.gameObject.SetActive(true);
        }
        Game.shipControl.gameObject.SetActive(false);

        //Game.main.debugText.text = "Designing Ship";
        //Game.main.debugText.color = Color.green;        
        SetDesignShip(Game.playerShip);
        Game.Pause();
    }
    
    public void OnDisable() {
        cursor.gameObject.SetActive(false);
        if (designShip != null) {
            designShip.form.tiles.EnableRendering();
        }
        Game.shipControl.gameObject.SetActive(true);
        //Game.main.debugText.text = "";
        //Game.main.debugText.color = Color.white;
        Game.Unpause();
    }

    void SetDesignShip(Ship ship) {
        if (designShip != null) {
            designShip.form.tiles.EnableRendering();
        }

        designShip = ship;
        designShip.form.tiles.DisableRendering();
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
            if (Block.Is<Floor>(existingBlock)) 
                return true;
            if (Block.Is<Wall>(existingBlock) && cursorBlock.type.canFitInsideWall)
                return true;
        }

        return false;
    }

    bool IsValidPlacement(IntVector2 targetBlockPos, BlueprintBlock cursorBlock, Block adjoiningBlock) {
        if (adjoiningBlock == null) return false;
        
        for (var i = 0; i < cursorBlock.Width; i++) {
            for (var j = 0; j < cursorBlock.Height; j++) {
                foreach (var block in designShip.form.blueprint.blocks[targetBlockPos.x+i, targetBlockPos.y+j]) {
                    //Debug.LogFormat("{0} {1} {2}", cursorBlock.type.name, block.type.name, CanFitInto(cursorBlock, block));
                    if (!CanFitInto(cursorBlock, block))
                        return false;
                }
            }
        }
        
        return true;
    }

    void Update() {
        var selectedType = MainUI.blockSelector.selectedType;
        var bp = new IntVector2(0, 0);
        if (cursor.blocks.Topmost(bp).type != selectedType) {
            cursor.blocks.RemoveSurface(bp);
            cursor.blocks[bp, selectedType.blockLayer] = new BlueprintBlock(selectedType);
        }

        cursor.transform.position = Game.mousePos;
        var cursorBlock = (BlueprintBlock)cursor.blocks.Topmost(bp);
        var targetBlockPos = designShip.form.WorldToBlockPos(Game.mousePos);
        var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, targetBlockPos);        
            
        UpdateRotation(cursorBlock, targetBlockPos, adjoiningBlock);

        var worldPos = designShip.form.BlockToWorldPos(targetBlockPos);
        cursor.transform.position = new Vector3(worldPos.x, worldPos.y, designShip.form.transform.position.z-1);
        cursor.transform.rotation = designShip.form.transform.rotation;
        
        var isValid = IsValidPlacement(targetBlockPos, cursorBlock, adjoiningBlock);

        if (isValid) {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.green;
        } else {
            foreach (var renderer in cursor.tiles.MeshRenderers)
                renderer.material.color = Color.red;
        }
        
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(KeyCode.F1)) {
            gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0) && isValid) {            
            designShip.blueprintBlocks[targetBlockPos, cursorBlock.layer] = new BlueprintBlock(cursorBlock);
            //designShip.blocks[targetBlockPos, cursorBlock.layer] = new Block(cursorBlock);
        } else if (Input.GetMouseButton(1)) {
            designShip.blueprintBlocks.RemoveSurface(targetBlockPos);
            designShip.blocks.RemoveSurface(targetBlockPos);
        }
    }
}
