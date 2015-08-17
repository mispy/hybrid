using UnityEngine;
using System;
using System.Collections;

public class Thruster : BlockType {
	[HideInInspector]
	public ParticleSystem ps;
	[HideInInspector]
	public Ship ship;
	public Block block;

	public bool isFiring = false;
	public bool isFiringAttitude = false;

	void Start() {
		ship = GetComponentInParent<Ship>();
		ps = GetComponentInChildren<ParticleSystem>();
		block = ship.BlockAtWorldPos(transform.position);
	}
	
	public void Fire() {		
		isFiringAttitude = false;
		isFiring = true;
		CancelInvoke("Stop");
		Invoke("Stop", 0.1f);
	}

	public void FireAttitude() {
		isFiring = false;
		isFiringAttitude = true;
		CancelInvoke("Stop");
		Invoke("Stop", 0.1f);
	}

	public void Stop() {
		isFiring = false;
		isFiringAttitude = false;
	}

	void Update() {
		if (isFiring || isFiringAttitude) {
			ps.Emit(1);
		}
	}

	void FixedUpdate() {
		if (isFiring) {
			ship.rigidBody.AddForce(-transform.up * Math.Min(ship.rigidBody.mass * 10, Block.typeByName["Wall"].mass * 1000) * Time.fixedDeltaTime * 300f);
		} else if (isFiringAttitude) {			
			var dist = transform.localPosition - ship.localCenter;
			var force = Math.Min(ship.rigidBody.mass * 10, Block.typeByName["Wall"].mass * 1000) * Time.fixedDeltaTime * 300f;
			
			if (dist.x > 0) {
				ship.rigidBody.AddRelativeTorque(Vector3.forward * force);
			} else {
				ship.rigidBody.AddRelativeTorque(Vector3.back * force);
			}
		}
	}
}
