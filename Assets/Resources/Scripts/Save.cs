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
		data.typeName = block.type.name;
		data.orientation = (int)block.orientation;
		return data;
	}

	public static ShipData Serialize(Ship ship) {
		var data = new ShipData();
		data.name = ship.name;
		data.position = ship.transform.position;
		data.rotation = ship.transform.rotation;
		data.velocity = ship.rigidBody.velocity;
		data.angularVelocity = ship.rigidBody.angularVelocity;

		data.blocks = new BlockData[ship.blocks.Count];
		data.blueprintBlocks = new BlockData[ship.blueprint.blocks.Count];

		var allBlocks = ship.blocks.AllBlocks.ToArray();
		for (var i = 0; i < allBlocks.Length; i++) {
			data.blocks[i] = Save.Serialize(allBlocks[i]);
		}

		var blueBlocks = ship.blueprint.blocks.AllBlocks.ToArray();
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


		data.player.position = Crew.player.transform.position;
		data.player.rotation = Crew.player.transform.rotation;
		data.player.velocity = Crew.player.rigidBody.velocity;
		data.player.angularVelocity = Crew.player.rigidBody.angularVelocity;
		
		return data;
	}

	public static Block Load(BlockData data) {
		var block = new Block(Block.types[data.typeName]);
		block.orientation = (Orientation)data.orientation;
		return block;
	}

	public static Ship LoadShip(ShipData data) {
		var shipObj = Pool.For("Ship").TakeObject();
		var ship = shipObj.GetComponent<Ship>();
		ship.name = data.name;

		foreach (var blockData in data.blocks) {
			ship.blocks[blockData.x, blockData.y] = Save.Load(blockData);
		}

		foreach (var blockData in data.blueprintBlocks) {
			ship.blueprint.blocks[blockData.x, blockData.y] = new BlueprintBlock(Save.Load(blockData));
		}

		ship.transform.position = data.position;
		ship.transform.rotation = data.rotation;
		ship.rigidBody.velocity = data.velocity;
		ship.rigidBody.angularVelocity = data.angularVelocity;

		
		shipObj.SetActive(true);
		return ship;
	}

	public static Crew Load(CrewData crewData) {
		Crew.player.transform.position = crewData.position;
		Crew.player.transform.rotation = crewData.rotation;
		Crew.player.rigidBody.velocity = crewData.velocity;
		Crew.player.rigidBody.angularVelocity = crewData.angularVelocity;
		return Crew.player;
	}

	public static void SaveGame() {
		var path = Application.dataPath + "/Saves/main.xml";
		var data = Serialize(Game.main);
		var serializer = new XmlSerializer(typeof(GameData));
		
		using (var stream = new FileStream(path, FileMode.Create)) {
			serializer.Serialize(stream, data);
		}
	}

	public static void LoadGame() {
		foreach (var ship in Ship.allActive.ToArray()) {
			Pool.Recycle(ship.gameObject);
		}


		var path = Application.dataPath + "/Saves/main.xml";
		var serializer = new XmlSerializer(typeof(GameData));

		GameData data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = serializer.Deserialize(stream) as GameData;
		}

		foreach (var shipData in data.ships) {
			var ship = Save.LoadShip(shipData);
		}

		Crew.player = Save.Load(data.player);
	}
}

[Serializable]
public class GameData {
	public ShipData[] ships;
	public CrewData player;
}

[Serializable]
public class ShipData {
	public string name;
	public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;
	public BlockData[] blocks;
	public BlockData[] blueprintBlocks;
}

[Serializable]
public struct BlockData {
	public int x;
	public int y;
	public string typeName;
	public int orientation;
}

[Serializable]
public struct CrewData {
	public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;
}