using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MissileLauncher : PoolBehaviour {
	public Ship ship;
	public Block block;
	public Collider collider;

	void OnEnable() {
		ship = transform.parent.gameObject.GetComponent<Ship>();
		block = ship.BlockAtWorldPos(transform.position);
		collider = ship.colliders[block.pos].GetComponent<Collider>();
	}

	public void Fire(Vector3 worldPos) {	
		Debug.Log("firing!");
		var missile = Pool.For("Missile").TakeObject();
		missile.transform.position = transform.position;
		missile.transform.rotation = transform.rotation;
		var mcol = missile.GetComponent<BoxCollider>();
		var rigid = missile.GetComponent<Rigidbody>();
		missile.SetActive(true);
		rigid.velocity = ship.rigidBody.velocity;
		rigid.AddForce(-transform.up*0.2f);
		Physics.IgnoreCollision(collider, mcol);
		if (ship.shields) {
			Physics.IgnoreCollision(ship.shields.GetComponent<Collider>(), mcol, true);
			Physics.IgnoreCollision(mcol, ship.shields.GetComponent<Collider>(), true);
		}
	}
	
	void Update() {
		
	}
}
