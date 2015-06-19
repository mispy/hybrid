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
		ship.rigidBody.AddForce(-transform.up * Math.Min(ship.rigidBody.mass * 10, Block.types["wall"].mass * 1000) * 4.0f);
	}

	public void FireAttitude() {
		ps.Emit(1);

		var dist = transform.localPosition - ship.localCenter;
		var force = Math.Min(ship.rigidBody.mass * 10, Block.types["wall"].mass * 1000) * 2.0f;

		if (dist.x > 0) {
			ship.rigidBody.AddRelativeTorque(Vector3.forward * force);
		} else {
			ship.rigidBody.AddRelativeTorque(Vector3.back * force);
		}
	}
}
