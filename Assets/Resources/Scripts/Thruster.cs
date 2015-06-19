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
	}
	
	public void Fire() {		
		ps.Emit(1);
		ship.rigidBody.AddForce(-transform.up * Math.Min(ship.rigidBody.mass * 10, Block.types["wall"].mass * 1000));
	}
}
