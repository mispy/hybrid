using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class Explosive : PoolBehaviour
{
    public float explosionForce = 0.001f;
    public float explosionRadius = 2f;
    public float damageAmount = 10f;
    [HideInInspector]
    public Rigidbody rigid;
	public GameObject explosionPrefab;
    public BlockComponent originComp;
    public bool hasStarted = false;
    public bool shouldExplode = false;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        if (initial) {
            writer.Write(rigid.velocity);
            writer.Write(originComp);
        }

        writer.Write(shouldExplode);
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        if (initial) {
            rigid.velocity = reader.ReadVector3();
            originComp = reader.ReadComponent<ProjectileLauncher>();
            if (!SpaceNetwork.isServer)
                GetComponent<Collider>().enabled = false;
        }

        shouldExplode = reader.ReadBoolean();
        if (shouldExplode) Explode();
    }

    void Start() {
        var form = originComp.form;
        var mcol = GetComponent<Collider>();
        foreach (var collider in originComp.GetComponentsInChildren<Collider>()) {
            Physics.IgnoreCollision(collider, mcol);
        }
        if (form.shields && form.shields.isActive) {
            Physics.IgnoreCollision(form.shields.GetComponent<Collider>(), mcol, true);
            Physics.IgnoreCollision(mcol, form.shields.GetComponent<Collider>(), true);
        }
        hasStarted = true;
    }

    /*void FixedUpdate() {
        foreach (var form in Game.activeSector.blockforms) {
            if (form.BlocksAtWorldPos(rigidbody.position).Any())
                Explode();
        }
    }*/

	public void Explode() {
        var explosion = Pool.For(explosionPrefab).Attach<Transform>(Game.activeSector.transients);
        explosion.transform.position = rigid.position;

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

        var cols = Physics.OverlapSphere(rigid.position, explosionRadius, LayerMask.GetMask(new string[] { "Wall", "Floor", "Shields", "Crew" }));

		HashSet<Block> blocksToBreak = new HashSet<Block>();
		HashSet<Rigidbody> rigidBodies = new HashSet<Rigidbody>();
        HashSet<Rigidbody> shielded = new HashSet<Rigidbody>();

        foreach (var col in cols) {
            var shields = col.gameObject.GetComponent<Shields>();
            if (shields != null && shields != originComp.block.ship.shields) {
                shielded.Add(col.attachedRigidbody);
                shields.TakeDamage(damageAmount);
            }
        }

		foreach (var col in cols)
		{
			rigidBodies.Add(col.attachedRigidbody);

            if (shielded.Contains(col.attachedRigidbody)) continue;

            var crew = col.gameObject.GetComponent<CrewBody>();
            if (crew != null) {
                crew.TakeDamage(damageAmount);
                continue;
            }
                
                

			var form = col.attachedRigidbody.GetComponent<Blockform>();
			if (form == null || form == originComp.form) continue;

            foreach (var block in form.BlocksAtWorldPos(col.transform.position))
                blocksToBreak.Add(block);
		}

		foreach (var block in blocksToBreak) {
            if (block.isDestroyed) continue;

            var startPos = block.ship.WorldToBlockPos(rigid.position);
            var damage = damageAmount;
            foreach (var pos in Util.LineBetween(startPos, block.pos)) {
                foreach (var between in block.ship.blocks.BlocksAtPos(pos)) {
                    if (between != block)
                        damage -= between.type.damageBuffer;
                }
            }

            damage = Mathf.Max(0, damage);
            block.health -= damage;
		}
		
		foreach (var rb in rigidBodies) {
            rb.AddExplosionForce(Math.Min(explosionForce, rb.mass*20), rigid.position, explosionRadius, 1, ForceMode.Impulse);
		}

        Pool.Recycle(gameObject);
	}
}
