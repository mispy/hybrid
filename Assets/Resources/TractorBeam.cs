using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TractorBeam : MonoBehaviour {
	public ParticleSystem beam;
	public Ship ship;

	public List<Collider> captured = new List<Collider>();

	void OnEnable() {
		ship = transform.parent.gameObject.GetComponent<Ship>();
		beam = GetComponent<ParticleSystem>();
		beam.enableEmission = false;
	}

	public void Stop() {
		beam.Clear();
		beam.enableEmission = false;
	}

	public void Fire(Vector3 worldPos) {	
		bool hasShields = ship.shields != null;
		Collider shieldCol = null;
		if (hasShields) {
			shieldCol = ship.shields.GetComponent<SphereCollider>();
			
			foreach (var col in captured) {
				if (col.enabled) {
					Physics.IgnoreCollision(col, shieldCol, false);
				}
			}
		}

		captured.Clear();

		var targetRotation = Quaternion.LookRotation((Vector3)worldPos - transform.position);
		if (targetRotation != transform.rotation) {
		}
		transform.rotation = targetRotation;
		beam.startLifetime = Vector3.Distance(transform.position, worldPos) / Math.Abs(beam.startSpeed);
		beam.enableEmission = true;
		var dir = (worldPos - transform.position);
		dir.Normalize();
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.10f, dir, Vector3.Distance(transform.position, worldPos));
		foreach (var hit in hits) {
			var rigid = hit.collider.attachedRigidbody;
			if (rigid != null) {
				if (rigid != ship.rigidBody) {
					rigid.AddForce(-dir * Math.Min(rigid.mass*2, Block.types["wall"].mass) * 10);
					if (hasShields)
						Physics.IgnoreCollision(hit.collider, shieldCol);
					captured.Add(hit.collider);
				}
			}
		}		
	}
	
	void Update() {
		
	}
}
