using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class DebugMenu : MonoBehaviour {
	public void SaveShip() {
		var ship = Crew.player.maglockShip;
		if (ship == null) return;
		var data = Save.Serialize(ship);

		var path = Application.dataPath + "/Ships/" + ship.name + ".xml";
		var serializer = new XmlSerializer(typeof(ShipData));
		
		using (var stream = new FileStream(path, FileMode.Create)) {
			serializer.Serialize(stream, data);
		}

		Game.main.BriefMessage("Saved " + path);
	}

	public void LoadShip() {
		var path = Application.dataPath + "/Ships/Test Ship.xml";//Directory.GetFiles(Application.dataPath + "/Ships/")[0];
		var serializer = new XmlSerializer(typeof(ShipData));
		
		ShipData data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = serializer.Deserialize(stream) as ShipData;
		}

		var ship = Save.Load(data);
		ship.transform.position = new Vector3(0, 0, 0);
	}

	public void MakeAsteroid(Vector2 pos) {		
		var sectorWidth = 200;
		var sectorHeight = 200;
		
		for (var i = 0; i < 10; i++) {
			var radius = Random.Range(10, 50);
			//var pos = new Vector2(Random.Range(-sectorWidth, sectorWidth), Random.Range(-sectorHeight, sectorHeight));
			if (Physics.OverlapSphere(pos, radius*Block.worldSize).Length == 0) {
				Generate.Asteroid(pos, radius);
				break;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			SaveShip();
		}

		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			LoadShip();
		}

		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			if (Ship.AtWorldPos(pz) == null)
				Generate.TestShip(pz);
		}

		if (Input.GetKeyDown(KeyCode.Alpha4)) {
			MakeAsteroid(pz);
		}
	}
}
