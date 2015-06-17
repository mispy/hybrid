using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Designer : MonoBehaviour {
	public Ship designShip;
	public Blueprint cursor;

	void Awake() {
		this.enabled = false;
	}

	public void StartDesigning() {
		if (cursor == null) {
			var cursorObj = Pool.For("Blueprint").TakeObject();
			cursorObj.name = "Cursor";
			cursor = cursorObj.GetComponent<Blueprint>();
			cursor.blocks[0, 0] = new Block(Block.types["wall"]);
			cursorObj.SetActive(true);
		} else {
			cursor.gameObject.SetActive(true);
		}

		//Game.main.debugText.text = "Designing Ship";
		//Game.main.debugText.color = Color.green;		
		SetDesignShip(Ship.ClosestTo(transform.position).First());

		this.enabled = true;
	}
	
	public void StopDesigning() {
		cursor.gameObject.SetActive(false);
		designShip.renderer.enabled = true;
		//Game.main.debugText.text = "";
		//Game.main.debugText.color = Color.white;

		this.enabled = false;
	}

	void SetDesignShip(Ship ship) {
		if (designShip != null) {
			designShip.renderer.enabled = true;
		}

		designShip = ship;
		designShip.renderer.enabled = false;
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
		var blockPos = designShip.WorldToBlockPos(worldPos);		

		if (adjoiningBlock == null && designShip.blueprint.blocks[blockPos] == null) {
			// new ship!
			var shipObj = Pool.For("Ship").TakeObject();
			SetDesignShip(shipObj.GetComponent<Ship>());
			shipObj.SetActive(true);
		}

		var block = new Block(cursor.blocks[0,0].type);
		block.orientation = cursor.blocks[0,0].orientation;
		designShip.blueprint.blocks[blockPos] = block;
	}

	void Update() {		
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		if (Input.GetKeyDown(KeyCode.Tab)) {
			var nextBlockType = Block.allTypes.IndexOf(cursor.blocks[0,0].type) + 1;
			if (nextBlockType >= Block.types.Count) nextBlockType = 0;
			cursor.blocks[0,0] = new Block(Block.allTypes[nextBlockType]);
		}

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

		if (Input.GetMouseButton(0)) {			
			PlaceBlock(pz, adjoiningBlock);
		} else if (Input.GetMouseButton(1)) {
			designShip.blueprint.blocks[blockPos] = null;
		}
	}
}
