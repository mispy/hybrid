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
			cursor.blocks[0, 0] = BlueprintBlock.Make<Wall>();
			cursor.blocks.EnableRendering();
			cursorObj.SetActive(true);
		} else {
			cursor.gameObject.SetActive(true);
		}

		//Game.main.debugText.text = "Designing Ship";
		//Game.main.debugText.color = Color.green;		
		if (Crew.player.maglockShip != null) {
			SetDesignShip(Crew.player.maglockShip);
		}
	}
	
	public void OnDisable() {
		cursor.gameObject.SetActive(false);
		if (designShip != null) {
			designShip.blocks.EnableRendering();
		}
		//Game.main.debugText.text = "";
		//Game.main.debugText.color = Color.white;
	}

	void SetDesignShip(Ship ship) {
		if (designShip != null) {
			designShip.blocks.EnableRendering();
		}

		designShip = ship;
		designShip.blocks.DisableRendering();
	}

	Block FindAdjoiningBlock(Vector2 worldPos, IntVector3 blockPos) {
		var neighborBlocks = new List<Block>();
		foreach (var bp in designShip.blueprint.blocks.Neighbors(blockPos)) {
			var block = designShip.blueprint.blocks[bp];
			if (block != null) 
				neighborBlocks.Add(block);
		}	

		if (neighborBlocks.Count == 0)
			return null;

		return neighborBlocks.OrderBy((block) => Vector2.Distance(worldPos, designShip.BlockToWorldPos(block.pos))).First();
	}

	void UpdateRotation(Block cursorBlock, IntVector3 targetBlockPos) {
		var adjoiningBlock = FindAdjoiningBlock(Game.mousePos, targetBlockPos);		

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
			cursor.blocks[0, 0] = block;
		}
	}

	void Update() {
		var selectedType = MainUI.blockSelector.selectedType;
		if (cursor.blocks[0,0].type != selectedType)
			cursor.blocks[0,0] = new Block(selectedType);

		cursor.transform.position = Game.mousePos;
		var cursorBlock = cursor.blocks[0,0];
		var targetBlockPos = designShip.WorldToBlockPos(Game.mousePos);
		UpdateRotation(cursorBlock, targetBlockPos);

		var worldPos = designShip.BlockToWorldPos(targetBlockPos);
		cursor.transform.position = new Vector3(worldPos.x, worldPos.y, designShip.transform.position.z-1);
		cursor.transform.rotation = designShip.transform.rotation;

		if (EventSystem.current.IsPointerOverGameObject())
			return;
		
		if (Input.GetMouseButton(0)) {			
			designShip.blueprint.blocks[targetBlockPos] = new BlueprintBlock(cursorBlock);
			designShip.blocks[targetBlockPos] = new Block(cursorBlock);
		} else if (Input.GetMouseButton(1)) {
			designShip.blueprint.blocks.RemoveTop(targetBlockPos);
			designShip.blocks.RemoveTop(targetBlockPos);
		}
	}
}
