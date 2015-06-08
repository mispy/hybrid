using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explosion : PoolBehaviour
{
	public void Explode(float radius, float force) {
		var multiplier = radius;
		var systems = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem system in systems)
		{
			system.startSize *= multiplier;
			system.startSpeed *= multiplier;
			system.startLifetime *= Mathf.Lerp(multiplier, 1, 0.5f);
			system.Clear();
			system.Play();
		}


		var cols = Physics.OverlapSphere(transform.position, radius);
		var rigidbodies = new List<Rigidbody>();
		foreach (var col in cols)
		{
			if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
			{
				rigidbodies.Add(col.attachedRigidbody);
			}
		}

		var toBreak = new List<Block>();

		foreach (var rb in rigidbodies) {
			var ship = rb.GetComponent<Ship>();		
			if (ship == null) continue;

			foreach (var block in ship.blocks.All) {
				var dist = ((Vector3)ship.BlockToWorldPos(block.pos) - transform.position).magnitude;
				if (dist < radius) {
					toBreak.Add(block);
				}
			}
		}

		foreach (var block in toBreak) {
			var ns = block.ship.BreakBlock(block);
			rigidbodies.Add(ns.rigidBody);
		}

		foreach (var rb in rigidbodies) {
			rb.AddExplosionForce(force, transform.position, radius, 1, ForceMode.Impulse);
		}
	}
}