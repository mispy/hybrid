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

	public RotatingTurret turret { get; private set; }
    public CooldownCharger charger { get; private set; }

	public override void OnRealize() {
		turret = GetComponent<RotatingTurret>();
        charger = GetComponent<CooldownCharger>();
	}

    public bool CanHit(Blockform target) {
        RaycastHit[] hits = Physics.RaycastAll(turret.TipPosition, (target.transform.position - transform.position).normalized);

        foreach (var hit in hits.OrderBy((hit) => Vector2.Distance(transform.position, hit.point))) {
            if (hit.collider.attachedRigidbody == target.rigidBody)
                return true;
            else if (hit.collider.attachedRigidbody != form.rigidBody)
                return false;
        }

        return false;
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
        bullet.transform.rotation = turret.stickyOutBit.transform.rotation;
        bullet.originComp = this;

        var rigid = bullet.GetComponent<Rigidbody>();
        rigid.velocity = form.rigidBody.velocity;
		rigid.velocity += turret.stickyOutBit.transform.up*launchVelocity;
	}
}
