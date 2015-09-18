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
	public static List<Ship> all = new List<Ship>();
	public static Dictionary<string, Ship> byId = new Dictionary<string, Ship>();

	public static ShipData RandomTemplate() {
		return templates[Util.GetRandom(templates.Keys.ToList())];
	}
	
	public static void LoadTemplates() {
		foreach (var path in Directory.GetFiles(Application.dataPath + "/Ships/", "*.xml")) {
			var data = Save.Load<ShipData>(path);
			var id = Util.GetIdFromPath(path);
			templates[id] = data;
		}
	}

	public static void LoadAll() {
		foreach (var path in Save.GetFiles("Ship")) {
			var data = Save.Load<ShipData>(path);
			var ship = ShipManager.Unpack(data);
			var id = Util.GetIdFromPath(path);
			ShipManager.all.Add(ship);
			ShipManager.byId[id] = ship;
		}
	}
	
	public static void SaveAll() {
		foreach (var sector in SectorManager.all) {
			Save.Dump(sector, Save.GetPath("Sector", sector.Id));
		}
	}

	public static void Add(Ship ship) {
		ShipManager.all.Add(ship);
	}

	public static Ship Unpack(ShipData data) {
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

	public static ShipData Pack(Ship ship) {
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
	public string name;
	/*public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;*/
	public BlockData[] blocks;
	public BlockData[] blueprintBlocks;
	public string sectorId;
}


public class Ship {
	public string name;
	public Sector sector;
	public BlockMap blocks;
	public BlockMap blueprintBlocks;
	public List<Crew> crew = new List<Crew>();
	public float scrapAvailable = 0f;
	

	public float jumpSpeed = 10f;
	public Sector destSector;
	public Vector2 galaxyPos;
	public Blockform form = null;
	public JumpShip jumpShip = null;

	public Ship() {
		crew = new List<Crew>();
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

	public void FoldJump(Sector destSector) {
		this.destSector = destSector;

		if (sector != null)
			sector.ships.Remove(this);
		sector = null;
	}

	public void JumpUpdate(float deltaTime) {
		if (destSector == null) return;

		var targetDir = (destSector.galaxyPos - galaxyPos).normalized;
		var dist = targetDir * jumpSpeed * deltaTime;

		if (Vector2.Distance(destSector.galaxyPos, galaxyPos) < dist.magnitude) {
			destSector.PlaceShip(this);
		} else {
			galaxyPos += dist;
		}

		if (jumpShip != null) jumpShip.SyncShip();
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

