using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ProjectileLauncher : BlockComponent {

	public float launchForce = 0.5f;
	public GameObject projectile;

	[HideInInspector]
	public float lastFireTime = 0f;
	public new Collider collider {
        get { return form.GetComponent<ShipCollision>().colliders[block.pos].GetComponent<Collider>(); }
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
		var bullet = Pool.For(projectile).Attach<Explosive>(Game.activeSector.transients, false);
       
		bullet.transform.position = turret.TipPosition;
		bullet.transform.rotation = transform.rotation;
        bullet.originShip = block.ship;
		bullet.gameObject.SetActive(true);
        var mcol = bullet.GetComponent<BoxCollider>();
        Physics.IgnoreCollision(collider, mcol);

        var rigid = bullet.GetComponent<Rigidbody>();
        rigid.velocity = form.rigidBody.velocity;
		rigid.AddForce(transform.up*launchForce);
	}
}
