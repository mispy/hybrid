using UnityEngine;
using System.Collections;

public class ShipMind : PoolBehaviour {
	public Ship ship;
	public Blockform form;
	public Ship nearestEnemy;

	// Use this for initialization
	void Start () {
		form = GetComponent<Blockform>();
		ship = GetComponent<Blockform>().ship;
	}

	bool IsEnemy(Ship otherShip) {
		return ship.faction.IsEnemy(otherShip.faction);
	}

	void UpdateTractors() {
		foreach (var tractor in form.GetBlockComponents<TractorBeam>()) {
			tractor.Stop();

			foreach (var target in tractor.GetViableTargets()) {
				if (target.CompareTag("Item")) {
					tractor.Fire(target.transform.position);
				}
			}
		}
	}

	void UpdateWeapons() {
		if (nearestEnemy == null) return;

		foreach (var launcher in form.GetBlockComponents<TorpedoLauncher>()) {
			launcher.AimTowards(nearestEnemy.form.transform.position);

			var hit = launcher.GetProbableHit();
			if (hit == null) continue;

			var otherShip = hit.gameObject.GetComponentInParent<Blockform>().ship;

			if (otherShip != null && IsEnemy(otherShip)) {
				launcher.Fire();
			}
		}
	}

	void UpdateMovement() {		
		Blockform target = nearestEnemy.form;

		if (target != null) {
			form.RotateTowards(target.transform.position);
			var dist = (target.transform.position - transform.position).magnitude;
			if (dist > 10f) {
				form.MoveTowards(target.transform.position);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (form.maglockedCrew.Count == 0 || form.ship == Game.playerShip) return;
		
		foreach (var other in Blockform.ClosestTo(transform.position)) {
			if (IsEnemy(other.ship)) {
				nearestEnemy = other.ship;
				break;
			}
		}			

		UpdateTractors();
	    UpdateWeapons();
		UpdateMovement();
	}
}
