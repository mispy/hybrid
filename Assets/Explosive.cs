using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionForce = 0.001f;
    public float explosionRadius = 2f;
	public GameObject explosionPrefab;

	[HideInInspector]
	public GameObject explosion;

    private IEnumerator OnCollisionEnter(Collision col)
    {
        if (enabled && col.contacts.Length > 0) {
            // compare relative velocity to collision normal - so we don't explode from a fast but gentle glancing collision
            float velocityAlongCollisionNormal =
                Vector3.Project(col.relativeVelocity, col.contacts[0].normal).magnitude;
            
            //if (velocityAlongCollisionNormal > detonationImpactVelocity)
            //{

                explosion = Pool.For(explosionPrefab).TakeObject();
                explosion.transform.position = col.contacts[0].point;
                explosion.transform.rotation = Quaternion.LookRotation(col.contacts[0].normal);
                explosion.SetActive(true);
                Explode(explosionRadius, explosionForce);
				Pool.Recycle(gameObject);
            //}
        }
        
        yield return null;
    }

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
			var shields = col.gameObject.GetComponent<Shields>();
			if (shields != null) {
				shields.TakeDamage(1f);
			}

			if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
			{
				rigidbodies.Add(col.attachedRigidbody);
			}
		}
		
		var toBreak = new List<Block>();
		
		foreach (var rb in rigidbodies) {
			var form = rb.GetComponent<Blockform>();        
			if (form == null) continue;

			foreach (var block in form.blocks.AllBlocks) {
				var dist = ((Vector3)form.BlockToWorldPos(block.pos) - transform.position).magnitude;
				if (dist < radius) {
					toBreak.Add(block);
				}
			}
		}
		
		foreach (var block in toBreak) {
			block.TakeDamage(10);
		}
		
		foreach (var rb in rigidbodies) {
			rb.AddExplosionForce(Math.Min(force, rb.mass*20), transform.position, radius, 1, ForceMode.Impulse);
		}
	}
}
