using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpBeacon : MonoBehaviour {
	[HideInInspector]
	public SpriteRenderer renderer;
	public List<JumpShip> ships = new List<JumpShip>();

	void Awake() {
		renderer = GetComponent<SpriteRenderer>();
	}

	public void PlaceShip(JumpShip jumpShip) {
		var size = renderer.bounds.size;
		jumpShip.transform.position = new Vector2(transform.position.x + size.x, transform.position.y + size.y + 0.3f*ships.Count);
		ships.Add(jumpShip);
		jumpShip.currentBeacon = this;
		jumpShip.destBeacon = null;
	}
}
