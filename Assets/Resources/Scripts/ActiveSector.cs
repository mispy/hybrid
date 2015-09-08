using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
	public float radius = 200f;

	public List<Blockform> blockforms;

	public GameObject leaveSectorMenu;

	public void UnloadSector() {
		foreach (var blockform in blockforms)
			blockform.gameObject.SetActive(false);
	}

	public Vector2 RandomEdge() {
		return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))*radius;
	}

	public void LoadSector(JumpBeacon beacon) {
		foreach (var jumpShip in beacon.ships) {
			var form = jumpShip.ship.LoadBlockform();
			form.transform.parent = transform;
			form.transform.position = RandomEdge();
			form.gameObject.SetActive(true);
		}
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
