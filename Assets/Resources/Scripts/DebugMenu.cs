using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

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

	public Ship LoadShip() {
		var path = Application.dataPath + "/Ships/Asteroid.xml";//Directory.GetFiles(Application.dataPath + "/Ships/")[0];
		var serializer = new XmlSerializer(typeof(ShipData));
		
		ShipData data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = serializer.Deserialize(stream) as ShipData;
		}

		var ship = Save.LoadShip(data);
		ship.transform.position = new Vector3(0, 0, 0);
		ship.gameObject.SetActive(true);
		return ship;
	}

	public void MakeAsteroid(Vector2 pos) {		
		
		var sectorSize = Game.main.activeSector.radius;
		for (var i = 0; i < 10; i++) {
			pos = new Vector2(Random.Range(-sectorSize, sectorSize), Random.Range(-sectorSize, sectorSize));
			for (var j = 0; j < 10; i++) {
				var radius = Random.Range(5, 10);
				if (Physics.OverlapSphere(pos, radius*Tile.worldSize).Length == 0) {
					Generate.Asteroid(pos, radius);
					break;
				}
			}
		}
	
	}

	public void SpawnCrew(Vector2 pos) {
		var crewObj = Pool.For("Crew").TakeObject();
		crewObj.transform.position = pos;
		crewObj.SetActive(true);
		crewObj.GetComponentInChildren<CrewMind>().myShip = Ship.ClosestTo(pos).First();
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

		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			SpawnCrew(pz);
		}
	}
}
