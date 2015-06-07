using UnityEngine;
using System;
using System.Collections;

public class Thruster : MonoBehaviour {
	public ParticleSystem ps;
	public Ship ship;
	public Block block;

	void OnEnable() {
		ship = transform.parent.gameObject.GetComponent<Ship>();
		ps = GetComponent<ParticleSystem>();
		block = ship.BlockAtWorldPos(transform.position);
		Vector2 worldOrient;
		if (block.orientation == Orientation.up) {
			worldOrient = ship.transform.TransformVector(Vector2.up);
		} else if (block.orientation == Orientation.down) {
			worldOrient = ship.transform.TransformVector(-Vector2.up);
		} else {
			worldOrient = ship.transform.TransformVector(block.orientation == Orientation.left ? Vector2.right : -Vector2.right);
		}
		
		transform.up = worldOrient;
	}
	
	public void Fire() {		
		ps.Emit(1);
		ship.rigidBody.AddForce(transform.up * Math.Min(ship.rigidBody.mass * 10, Block.types["wall"].mass * 1000));
	}
}
