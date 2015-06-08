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
		if (ship.shields != null) {
			var shieldCol = ship.shields.GetComponent<SphereCollider>();
			foreach (var col in captured) {
				Physics.IgnoreCollision(col, shieldCol, false);
			}
		}
	}

	public void Fire(Vector3 worldPos) {	
		Collider shieldCol = null;
		if (ship.shields != null)
			shieldCol = ship.shields.GetComponent<SphereCollider>();

		foreach (var col in captured) {
			if (col != null && col.attachedRigidbody != null) {
				col.attachedRigidbody.drag -= 5;
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
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.20f, dir, Vector3.Distance(transform.position, worldPos));
		foreach (var hit in hits) {
			var rigid = hit.collider.attachedRigidbody;
			if (rigid != null) {
				if (rigid != ship.rigidBody) {
					rigid.AddForce(-dir * Math.Min(rigid.mass*2, Block.types["wall"].mass) * 100);
					if (shieldCol != null)
						Physics.IgnoreCollision(hit.collider, shieldCol);
					hit.collider.attachedRigidbody.drag += 5;
					captured.Add(hit.collider);
				}
			}
		}		
	}
	
	void Update() {
		
	}
}
