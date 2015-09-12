using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpBeacon : MonoBehaviour {
	[HideInInspector]
	public SpriteRenderer renderer;
	public Sector sector;

	void Awake() {
		renderer = GetComponent<SpriteRenderer>();
	}

	public void Align(JumpShip jumpShip) {
		var size = renderer.bounds.size;
		jumpShip.transform.position = new Vector2(transform.position.x + size.x, transform.position.y + size.y + 0.3f*sector.ships.IndexOf(jumpShip.ship));
	}
}
