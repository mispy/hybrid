using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TorpedoLauncher : BlockType {
	[HideInInspector]
	public Ship ship;
	public Block block;
	[HideInInspector]
	public Collider collider;
	private float timeBetweenShots = 0.1f;
	private float lastFireTime = 0f;

	void OnEnable() {
		ship = GetComponentInParent<Ship>();
		block = ship.BlockAtWorldPos(transform.position);
		collider = ship.colliders[block.pos].GetComponent<Collider>();
	}

	public Collider GetProbableHit(float maxDistance = 50f) {
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.up, out hit, maxDistance);	
		return hit.collider;
	}

	public void Fire() {	
		if (Time.time - lastFireTime < timeBetweenShots)
			return;

		/*if (Util.TurretBlocked(ship, transform.position, worldPos, 0.3f)) {
			return;
		}

		var targetDir = (worldPos-transform.position).normalized;
		var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);*/

		lastFireTime = Time.time;


		var torpedo = Pool.For("Torpedo").TakeObject();
		torpedo.transform.position = transform.position;
		torpedo.transform.rotation = transform.rotation;
		var mcol = torpedo.GetComponent<BoxCollider>();
		var rigid = torpedo.GetComponent<Rigidbody>();
		torpedo.SetActive(true);
		rigid.velocity = ship.rigidBody.velocity;
		rigid.AddForce(transform.up*0.5f);
		Physics.IgnoreCollision(collider, mcol);
		if (ship.shields) {
			Physics.IgnoreCollision(ship.shields.GetComponent<Collider>(), mcol, true);
			Physics.IgnoreCollision(mcol, ship.shields.GetComponent<Collider>(), true);
		}
	}
}
