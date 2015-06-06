using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class Save {
	public static BlockData Serialize(Block block) {
		var data = new BlockData();
		data.x = block.pos.x;
		data.y = block.pos.y;
		data.type = block.type;
		data.orientation = (int)block.orientation;
		return data;
	}

	public static ShipData Serialize(Ship ship) {
		var data = new ShipData();
		data.position = ship.transform.position;
		data.rotation = ship.transform.rotation;

		data.blocks = new BlockData[ship.blocks.Count];
		data.blueprintBlocks = new BlockData[ship.blueprint.blocks.Count];

		var allBlocks = ship.blocks.All.ToArray();
		for (var i = 0; i < allBlocks.Length; i++) {
			data.blocks[i] = Save.Serialize(allBlocks[i]);
		}

		var blueBlocks = ship.blueprint.blocks.All.ToArray();
		for (var i = 0; i < blueBlocks.Length; i++) {
			data.blueprintBlocks[i] = Save.Serialize(blueBlocks[i]);
		}

		return data;
	}

	public static GameData Serialize(Game game) {
		var data = new GameData();
		data.ships = new ShipData[Ship.allActive.Count];
		for (var i = 0; i < data.ships.Length; i++) {
			data.ships[i] = Save.Serialize(Ship.allActive[i]);
		}
		return data;
	}

	public static void SaveGame() {
		var path = Application.persistentDataPath + "/main.xml";
		var data = Serialize(Game.main);
		var serializer = new XmlSerializer(typeof(GameData));

		using (var stream = new FileStream(path, FileMode.Truncate)) {
			serializer.Serialize(stream, data);
		}
	}

	public static Block Load(BlockData data) {
		var block = new Block(data.type);
		block.orientation = (Orientation)data.orientation;
		return block;
	}

	public static Ship Load(ShipData data) {
		var shipObj = Pool.ship.TakeObject();
		var ship = shipObj.GetComponent<Ship>();
		ship.transform.position = data.position;
		ship.transform.rotation = data.rotation;

		foreach (var blockData in data.blocks) {
			ship.blocks[blockData.x, blockData.y] = Save.Load(blockData);
		}

		foreach (var blockData in data.blueprintBlocks) {
			ship.blueprint.blocks[blockData.x, blockData.y] = Save.Load(blockData);
		}

		shipObj.SetActive(true);
		return ship;
	}

	public static void LoadGame() {
		foreach (var ship in Ship.allActive.ToArray()) {
			GameObject.Destroy(ship.gameObject);
		}

		var path = Application.persistentDataPath + "/main.xml";
		var serializer = new XmlSerializer(typeof(GameData));

		GameData data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = serializer.Deserialize(stream) as GameData;
		}

		foreach (var shipData in data.ships) {
			var ship = Save.Load(shipData);
		}
	}
}

[Serializable]
public class GameData {
	public ShipData[] ships;
}

[Serializable]
public class ShipData {
	public Vector2 position;
	public Quaternion rotation;
	public BlockData[] blocks;
	public BlockData[] blueprintBlocks;
}

[Serializable]
public struct BlockData {
	public int x;
	public int y;
	public int type;
	public int orientation;
}

[Serializable]
public struct CrewData {
	public Vector2 position;
}