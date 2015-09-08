using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class DebugMenu : MonoBehaviour {
	public void SaveShip() {
		var ship = Game.playerShip;
		if (ship == null) return;
		var data = ShipManager.Serialize(ship);
		var path = Application.dataPath + "/Ships/" + ship.name + ".xml";
		Save.Dump(data, path);
		Game.main.BriefMessage("Saved " + path);
	}

	public Ship LoadShip() {
		var path = Application.dataPath + "/Ships/Asteroid.xml";//Directory.GetFiles(Application.dataPath + "/Ships/")[0];
		var ship = ShipManager.Deserialize(Save.Load<ShipData>(path));
		return ship;
	}

	public void MakeAsteroid(Vector2 pos) {				
		for (var i = 0; i < 10; i++) {
			var radius = Random.Range(5, 10);
			if (Physics.OverlapSphere(pos, radius*Tile.worldSize).Length == 0) {
				Generate.Asteroid(pos, radius);
				break;
			}
		}
	}

	public void SpawnCrew(Vector2 pos) {
		var crewObj = Pool.For("Crew").TakeObject();
		crewObj.transform.position = pos;
		crewObj.SetActive(true);
		crewObj.GetComponentInChildren<CrewMind>().myShip = Blockform.ClosestTo(pos).First().ship;
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
			if (Blockform.AtWorldPos(pz) == null)
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
