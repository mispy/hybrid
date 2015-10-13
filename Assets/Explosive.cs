using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Explosive : MonoBehaviour
{
    public float explosionForce = 0.001f;
    public float explosionRadius = 2f;
    Rigidbody rigidbody;
	public GameObject explosionPrefab;
    public Ship originShip;

	[HideInInspector]
	public GameObject explosion;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }
                 

    private IEnumerator OnCollisionEnter(Collision col)
    {
        if (enabled && col.contacts.Length > 0) {
            // compare relative velocity to collision normal - so we don't explode from a fast but gentle glancing collision
            float velocityAlongCollisionNormal =
                Vector3.Project(col.relativeVelocity, col.contacts[0].normal).magnitude;
            
            //if (velocityAlongCollisionNormal > detonationImpactVelocity)
            //{

                Explode();
            //}
        }
        
        yield return null;
    }

    /*void FixedUpdate() {
        foreach (var form in Game.activeSector.blockforms) {
            if (form.BlocksAtWorldPos(rigidbody.position).Any())
                Explode();
        }
    }*/

	public void Explode() {
        explosion = Pool.For(explosionPrefab).TakeObject();
        explosion.transform.position = rigidbody.position;
        //explosion.transform.rotation = Quaternion.LookRotation(col.contacts[0].normal);
        explosion.SetActive(true);

		var multiplier = explosionRadius;
		var systems = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem system in systems)
		{
			system.startSize *= multiplier;
			system.startSpeed *= multiplier;
			system.startLifetime *= Mathf.Lerp(multiplier, 1, 0.5f);
			system.Clear();
			system.Play();
		}		

        var cols = Physics.OverlapSphere(rigidbody.position, explosionRadius, LayerMask.GetMask(new string[] { "Wall", "Floor", "Shields" }));

		HashSet<Block> blocksToBreak = new HashSet<Block>();
		HashSet<Rigidbody> rigidBodies = new HashSet<Rigidbody>();
        HashSet<Rigidbody> shielded = new HashSet<Rigidbody>();

        foreach (var col in cols) {
            var shields = col.gameObject.GetComponent<Shields>();
            if (shields != null) {
                shielded.Add(col.attachedRigidbody);
                shields.TakeDamage(1f);
            }
        }

		foreach (var col in cols)
		{
			rigidBodies.Add(col.attachedRigidbody);

            if (shielded.Contains(col.attachedRigidbody)) continue;

			var form = col.attachedRigidbody.GetComponent<Blockform>();
			if (form == null || form == originShip.form) continue;

            foreach (var block in form.BlocksAtWorldPos(col.transform.position))
                blocksToBreak.Add(block);

		}

		foreach (var block in blocksToBreak) {
            if (block.IsDestroyed) continue;

            var startPos = block.ship.form.WorldToBlockPos(rigidbody.position);
            var damage = 10f;
            foreach (var pos in Util.LineBetween(startPos, block.pos)) {
                foreach (var between in block.ship.blocks.BlocksAtPos(pos)) {
                    if (between != block)
                        damage -= between.type.damageBuffer;
                }
            }

            damage = Mathf.Max(0, damage);
            block.ship.form.damage.DamageBlock(block, damage);
		}
		
		foreach (var rb in rigidBodies) {
            rb.AddExplosionForce(Math.Min(explosionForce, rb.mass*20), rigidbody.position, explosionRadius, 1, ForceMode.Impulse);
		}

        Pool.Recycle(gameObject);
	}
}
