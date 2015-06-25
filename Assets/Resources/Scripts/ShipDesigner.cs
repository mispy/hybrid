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
			cursor.blocks[0, 0] = new Block(Block.types["wall"]);
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
			designShip.blueprint.blocks.DisableRendering();
			designShip.blocks.EnableRendering();
		}
		//Game.main.debugText.text = "";
		//Game.main.debugText.color = Color.white;
	}

	void SetDesignShip(Ship ship) {
		if (designShip != null) {
			designShip.blueprint.blocks.DisableRendering();
			designShip.blocks.EnableRendering();
		}

		designShip = ship;
		designShip.blocks.DisableRendering();
		designShip.blueprint.blocks.EnableRendering();
	}

	Block FindAdjoiningBlock(Vector2 worldPos, IntVector2 blockPos) {
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

	public void PlaceBlock(Vector2 worldPos, Block adjoiningBlock = null) {
		if (designShip == null || (adjoiningBlock == null && designShip.blueprint.blocks[designShip.WorldToBlockPos(worldPos)] == null)) {
			// new ship!
			var shipObj = Pool.For("Ship").TakeObject();
			SetDesignShip(shipObj.GetComponent<Ship>());
			shipObj.SetActive(true);
		}

		var blockPos = designShip.WorldToBlockPos(worldPos);
		var block = new BlueprintBlock(cursor.blocks[0,0].type);
		block.orientation = cursor.blocks[0,0].orientation;
		designShip.blueprint.SetBlock(blockPos, block);
	}


	void UpdateWithShip() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		var blockPos = designShip.WorldToBlockPos(pz);
		var adjoiningBlock = FindAdjoiningBlock(pz, blockPos);
		
		cursor.transform.position = pz;
		if (adjoiningBlock != null) {
			cursor.transform.position = designShip.BlockToWorldPos(blockPos);
			cursor.transform.rotation = designShip.transform.rotation;
			
			Orientation ori;
			if (blockPos.x < adjoiningBlock.pos.x) {
				ori = Orientation.left;
			} else if (blockPos.x > adjoiningBlock.pos.x) {
				ori = Orientation.right;
			} else if (blockPos.y > adjoiningBlock.pos.y) {
				ori = Orientation.up;
			} else {
				ori = Orientation.down;
			}
			
			if (ori != cursor.blocks[0, 0].orientation) {
				var block = cursor.blocks[0, 0];
				block.orientation = ori;
				cursor.blocks[0, 0] = block;
			}
		}

		if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (Input.GetMouseButton(0)) {			
			PlaceBlock(pz, adjoiningBlock);
		} else if (Input.GetMouseButton(1)) {
			designShip.blueprint.blocks[blockPos] = null;
		}
	}

	void UpdateNoShip() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		cursor.transform.position = pz;

		if (Input.GetMouseButtonDown(0)) {
			PlaceBlock(pz);
		}
	}

	void Update() {		
		var selectedType = MainUI.blockSelector.selectedType;
		if (cursor.blocks[0,0].type != selectedType)
			cursor.blocks[0,0] = new Block(selectedType);

		if (designShip != null) {
			UpdateWithShip();
		} else {
			UpdateNoShip();
		}
	}
}
