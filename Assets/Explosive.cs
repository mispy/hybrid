using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
		


		var cols = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask(new string[] { "Wall", "Floor", "Shields" }));

		HashSet<Block> blocksToBreak = new HashSet<Block>();
		HashSet<Rigidbody> rigidBodies = new HashSet<Rigidbody>();
		foreach (var col in cols)
		{
			rigidBodies.Add(col.attachedRigidbody);

			var form = col.attachedRigidbody.GetComponent<Blockform>();
			if (form == null) continue;

			var shields = col.gameObject.GetComponent<Shields>();
			if (shields != null)
				shields.TakeDamage(1f);
			else
				foreach (var block in form.BlocksAtWorldPos(col.transform.position))
                    blocksToBreak.Add(block);

		}

		foreach (var block in blocksToBreak) {
            if (block.IsDestroyed) continue;
            block.ship.form.damage.DamageBlock(block, 10);
		}
		
		foreach (var rb in rigidBodies) {
			rb.AddExplosionForce(Math.Min(force, rb.mass*20), transform.position, radius, 1, ForceMode.Impulse);
		}
	}
}
