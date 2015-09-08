using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
	public Sector sector;
	public float radius = 200f;

	public List<Blockform> blockforms;

	public GameObject leaveSectorMenu;
	
	public void LoadSector(Sector sector) {
		foreach (var ship in ShipManager.all) {
			if (ship.sector == sector) {
				var form = ship.LoadBlockform();
				form.transform.parent = transform;
				form.transform.position = RandomEdge();
				form.gameObject.SetActive(true);
			}
		}

		var camera = GetComponentInChildren<Camera>();
		var pos = Game.playerShip.form.transform.position;
		camera.transform.position = new Vector3(pos.x, pos.y, camera.transform.position.z);
		camera.transform.parent = Game.playerShip.form.transform;
	}

	public void UnloadSector() {
		foreach (var blockform in blockforms)
			blockform.gameObject.SetActive(false);
	}

	public Vector2 RandomEdge() {
		return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))*radius;
	}

	public bool IsOutsideBounds(Vector3 pos) {
		return pos.magnitude > radius;
	}

	void Update() {
		if (IsOutsideBounds(Game.playerShip.form.transform.position)) {
			leaveSectorMenu.SetActive(true);
		} else {
			leaveSectorMenu.SetActive(false);
		}
	}
}
