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
	public Blockform form { get; private set; }
	public Collider collider { get; private set; }
	public RotatingTurret turret { get; private set; }
    public CooldownCharger charger { get; private set; }

	void Start() {
		form = GetComponentInParent<Blockform>();
		collider = form.GetComponent<ShipCollision>().colliders[block.pos].GetComponent<Collider>();
		turret = GetComponent<RotatingTurret>();
        charger = GetComponent<CooldownCharger>();
	}
	
	public Collider GetProbableHit(float maxDistance = 150f) {
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.up, out hit, maxDistance);    
		return hit.collider;
	}
	
    public void OnFire() {
        Fire();
    }

	public void Fire() {    
        if (!charger.isReady)
            return;

		if (turret.isBlocked)
			return;
		
		charger.Discharge();

		// Pew pew!
		var bullet = Pool.For(projectile).Take<Explosive>();
		bullet.transform.position = turret.TipPosition;
		bullet.transform.rotation = transform.rotation;
        bullet.originShip = block.ship;
		var mcol = bullet.GetComponent<BoxCollider>();
		var rigid = bullet.GetComponent<Rigidbody>();
		bullet.gameObject.SetActive(true);
		rigid.velocity = form.rigidBody.velocity;
		rigid.AddForce(transform.up*launchForce);

		// We want to stop the projectile colliding with the launcher block itself
		// and also the shields of the ship
		Physics.IgnoreCollision(collider, mcol);
		if (form.shields && form.shields.isActive) {
			Physics.IgnoreCollision(form.shields.GetComponent<Collider>(), mcol, true);
			Physics.IgnoreCollision(mcol, form.shields.GetComponent<Collider>(), true);
		}
	}

    void Update() {
        if (turret.fixedTargetForm != null)
            Fire();
    }
}
