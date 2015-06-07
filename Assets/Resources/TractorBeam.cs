using UnityEngine;
using System;
using System.Collections;

public class TractorBeam : MonoBehaviour {
	public ParticleSystem beam;
	public Ship ship;

	void OnEnable() {
		ship = transform.parent.gameObject.GetComponent<Ship>();
		beam = GetComponent<ParticleSystem>();
	}

	public void Fire(Vector3 worldPos) {		
		var targetRotation = Quaternion.LookRotation((Vector3)worldPos - transform.position);
		if (targetRotation != transform.rotation) {
			beam.Clear();
		}
		beam.startLifetime = Vector3.Distance(transform.position, worldPos) / Math.Abs(beam.startSpeed);
		var dir = (worldPos - transform.position);
		dir.Normalize();
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.05f, dir, Vector3.Distance(transform.position, worldPos));
		foreach (var hit in hits) {
			if (hit.collider.attachedRigidbody != null) {
				if (hit.collider.attachedRigidbody != ship.rigidBody) {
					hit.collider.attachedRigidbody.AddForce(-dir * Block.types["wall"].mass * 10);
				}
			}
		}
	}
	
	void Update() {
	
	}
}
