using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MissileLauncher : PoolBehaviour {
	public Ship ship;
	public Block block;
	public Collider collider;
	private float timeBetweenShots = 0.1f;
	private float lastFireTime = 0f;

	void OnEnable() {
		ship = transform.parent.gameObject.GetComponent<Ship>();
		block = ship.BlockAtWorldPos(transform.position);
		collider = ship.colliders[block.pos].GetComponent<Collider>();
	}

	public void Fire(Vector3 worldPos) {	
		if (Time.time - lastFireTime < timeBetweenShots)
			return;

		if (Util.TurretBlocked(ship, transform.position, worldPos, 0.3f)) {
			return;
		}

		lastFireTime = Time.time;

		var targetDir = (worldPos-transform.position).normalized;
		var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);

		var missile = Pool.For("Missile").TakeObject();
		missile.transform.position = transform.position;
		missile.transform.rotation = targetRotation;
		var mcol = missile.GetComponent<BoxCollider>();
		var rigid = missile.GetComponent<Rigidbody>();
		missile.SetActive(true);
		rigid.velocity = ship.rigidBody.velocity;
		rigid.AddForce(targetDir*0.2f);
		Physics.IgnoreCollision(collider, mcol);
		if (ship.shields) {
			Physics.IgnoreCollision(ship.shields.GetComponent<Collider>(), mcol, true);
			Physics.IgnoreCollision(mcol, ship.shields.GetComponent<Collider>(), true);
		}
	}
	
	void Update() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
	}
}
