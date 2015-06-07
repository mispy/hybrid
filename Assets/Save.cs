using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class Save {
	public static GameData Serialize(Game game) {
		var data = new GameData();
		/*data.ships = new ShipData[Ship.allActive.Count];
		for (var i = 0; i < data.ships.Length; i++) {
			data.ships[i] = Save.Serialize(Ship.allActive[i]);
		}*/


		data.player.position = Crew.player.transform.position;
		data.player.rotation = Crew.player.transform.rotation;
		data.player.velocity = Crew.player.rigidBody.velocity;
		data.player.angularVelocity = Crew.player.rigidBody.angularVelocity;
		
		return data;
	}

	public static void SaveGame() {
		var path = Application.dataPath + "/Saves/main.xml";
		var data = Serialize(Game.main);
		var serializer = new XmlSerializer(typeof(GameData));

		using (var stream = new FileStream(path, FileMode.Create)) {
			serializer.Serialize(stream, data);
		}
	}

	public static Crew Load(CrewData crewData) {
		Crew.player.transform.position = crewData.position;
		Crew.player.transform.rotation = crewData.rotation;
		Crew.player.rigidBody.velocity = crewData.velocity;
		Crew.player.rigidBody.angularVelocity = crewData.angularVelocity;
		return Crew.player;
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

		Crew.player = Save.Load(data.player);
	}
}

[Serializable]
public class GameData {
	public CrewData player;
}

[Serializable]
public struct CrewData {
	public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;
}