using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

public static class ShipManager {
	public static Dictionary<string, ShipData> templates = new Dictionary<string, ShipData>();


	public static ShipData Template(string name) {
		return templates[name];
	}
	
	public static ShipData RandomTemplate() {
		return templates[Util.GetRandom(templates.Keys.ToList())];
	}
	
	public static void LoadTemplates() {
		foreach (var path in Directory.GetFiles(Application.dataPath + "/Ships/", "*.*.xml")) {
			var data = Save.Load<ShipData>(path);
			templates[data.id] = data;
		}
	}
	
	public static Ship Create(ShipData template) {
		return ShipManager.Deserialize(template);
	}

	public static Ship Deserialize(ShipData data) {
		var ship = new Ship();
		ship.name = data.name;
		
		foreach (var blockData in data.blocks) {
			var block = BlockManager.Deserialize(blockData);
			ship.blocks[blockData.x, blockData.y, block.layer] = block;
		}
		
		foreach (var blockData in data.blueprintBlocks) {
			var block = new BlueprintBlock(BlockManager.Deserialize(blockData));
			ship.blueprintBlocks[blockData.x, blockData.y, block.layer] = block;
		}
		return ship;
	}

	public static ShipData Serialize(Ship ship) {
		var data = new ShipData();
		data.name = ship.name;

		data.blocks = new BlockData[ship.blocks.Count];
		data.blueprintBlocks = new BlockData[ship.blueprintBlocks.Count];
		
		var allBlocks = ship.blocks.AllBlocks.ToArray();
		for (var i = 0; i < allBlocks.Length; i++) {
			data.blocks[i] = BlockManager.Serialize(allBlocks[i]);
		}
		
		var blueBlocks = ship.blueprintBlocks.AllBlocks.ToArray();
		for (var i = 0; i < blueBlocks.Length; i++) {
			data.blueprintBlocks[i] = BlockManager.Serialize(blueBlocks[i]);
		}
		
		return data;
	}
}
[Serializable]
public class ShipData {
	public string id;
	public string name;
	public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;
	public BlockData[] blocks;
	public BlockData[] blueprintBlocks;
}


public class Ship {
	public string name;
	public BlockMap blocks;
	public BlockMap blueprintBlocks;
	public float scrapAvailable = 0f;

	public Blockform form;

	public Ship() {
		blocks = new BlockMap();
		blueprintBlocks = new BlockMap();
		blocks.OnBlockAdded += OnBlockAdded;
	}

	public Blockform LoadBlockform() {
		var blockform = Pool.For("Blockform").Take<Blockform>();
		blockform.Initialize(this);
		this.form = blockform;
		return blockform;
	}

	public void OnBlockAdded(Block newBlock) {
		newBlock.ship = this;
	}

	public void SetBlock<T>(int x, int y) {
		var block = Block.Make<T>();
		blocks[x, y, block.layer] = block;
		var block2 = BlueprintBlock.Make<T>();
		blueprintBlocks[x, y, block2.layer] = block2;
	}
	
	public void SetBlock(IntVector2 pos, BlockType type) {
		var block = new Block(type);
		blocks[pos, block.layer] = block;
		var block2 = new BlueprintBlock(type);
		blueprintBlocks[pos, block2.layer] = block2;
	}
	
	public void SetBlock(int x, int y, BlockType type) {
		var block = new Block(type);
		blocks[x, y, block.layer] = block;
		var block2 = new BlueprintBlock(type);
		blueprintBlocks[x, y, block2.layer] = block2;
	}
	
	public void SetBlock<T>(int x, int y, Orientation orientation) {
		var block = Block.Make<T>();
		block.orientation = orientation;
		blocks[x, y, block.layer] = block;
		
		var block2 = BlueprintBlock.Make<T>();
		block2.orientation = orientation;
		blueprintBlocks[x, y, block2.layer] = block2;
	}

}

