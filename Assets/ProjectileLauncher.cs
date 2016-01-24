using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ProjectileLauncher : BlockComponent {

	public float launchVelocity = 80f;
	public GameObject projectile;

	[HideInInspector]
	public float lastFireTime = 0f;
	public IEnumerable<Collider> colliders {
        get { 
            var col = form.GetComponent<ShipCollision>();
            for (var i = 0; i < block.Width; i++) {
                for (var j = 0; j < block.Height; j++) {
                    var pos = new IntVector2(block.pos.x + i, block.pos.y + j);
                    if (col.colliders.ContainsKey(pos))
                        yield return col.colliders[pos];
                }
            }
        }
    }

	public RotatingTurret turret { get; private set; }
    public CooldownCharger charger { get; private set; }

	public override void OnRealize() {
		turret = GetComponent<RotatingTurret>();
        charger = GetComponent<CooldownCharger>();
	}
	
	public Collider GetProbableHit(float maxDistance = 150f) {
		RaycastHit hit;
        Physics.Raycast(transform.position, transform.up, out hit, maxDistance, LayerMask.GetMask(new string[] { "Wall", "Floor"}), QueryTriggerInteraction.Ignore);    
		return hit.collider;
	}
	
    public void OnFire() {
        if (!charger.isReady)
            return;
        
        if (turret.isBlocked)
            return;

        charger.Discharge();

        if (!SpaceNetwork.isServer)
            SpaceNetwork.ServerCall(this, "OnFire");
        else
            Fire();
    }

	void Fire() {    
		// Pew pew!
		var bullet = Pool.For(projectile).Attach<Explosive>(Game.activeSector.transients);
       
		bullet.transform.position = turret.TipPosition;
		bullet.transform.rotation = transform.rotation;
        bullet.originComp = this;

        var rigid = bullet.GetComponent<Rigidbody>();
        rigid.velocity = form.rigidBody.velocity;
		rigid.velocity += transform.up*launchVelocity;
	}
}
