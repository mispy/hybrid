using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class Save {
	public static void SaveGame() {
		var path = Application.dataPath + "/Saves/main.xml";
	}

	public static void Dump<T>(T data, string path) {
		var serializer = new XmlSerializer(typeof(T));

		using (var stream = new FileStream(path, FileMode.Create)) {
			serializer.Serialize(stream, data);
		}
	}

	public static T Load<T>(string path) {
		var serializer = new XmlSerializer(typeof(T));

		T data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = (T)serializer.Deserialize(stream);
		}
		return data;
	}
}

[Serializable]
public class GameData {
	public ShipData[] ships;
}

[Serializable]
public struct CrewData {
	public Vector2 position;
	public Quaternion rotation;
	public Vector2 velocity;
	public Vector3 angularVelocity;
}