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
		var path = Directory.GetFiles(Application.dataPath + "/Ships/")[0];
		var serializer = new XmlSerializer(typeof(ShipData));
		
		ShipData data;
		using (var stream = new FileStream(path, FileMode.Open))
		{
			data = serializer.Deserialize(stream) as ShipData;
		}

		var ship = Save.Load(data);
		ship.transform.position = new Vector3(0, 0, 0);
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
	}
}
