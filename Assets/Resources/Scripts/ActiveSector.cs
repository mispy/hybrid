using UnityEngine;
using System.Collections;

public class ActiveSector : MonoBehaviour {
	public float radius = 200f;

	public void UnloadSector() {
		foreach (var ship in Ship.allActive)
			ship.gameObject.SetActive(false);
	}

	public Vector2 RandomEdge() {
		return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))*radius;
	}

	public void LoadSector(JumpBeacon beacon) {
		foreach (var jumpShip in beacon.ships) {
			Debug.Log(jumpShip.ship);
			jumpShip.ship.transform.parent = transform;
			jumpShip.ship.transform.position = RandomEdge();
			jumpShip.ship.gameObject.SetActive(true);
		}
	}
}
