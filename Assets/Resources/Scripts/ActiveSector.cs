using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
	public Sector sector;
	public float radius = 200f;

	public List<Blockform> blockforms;

	public GameObject leaveSectorMenu;

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
