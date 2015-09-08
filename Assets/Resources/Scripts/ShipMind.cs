using UnityEngine;
using System.Collections;



public class ShipTacticalMind : PoolBehaviour {
	public Blockform form;

	// Use this for initialization
	void Start () {
		form = GetComponent<Blockform>();
	}

	bool IsEnemy(Ship otherShip) {
		return (otherShip != form.ship);
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
		foreach (var launcher in form.GetBlockComponents<TorpedoLauncher>()) {
			var hit = launcher.GetProbableHit();
			if (hit == null) continue;

			var otherShip = hit.gameObject.GetComponentInParent<Ship>();

			if (otherShip != null && IsEnemy(otherShip)) {
				//launcher.Fire();
			}
		}
	}

	void UpdateMovement() {		
		Blockform target = null;
		
		foreach (var other in Blockform.ClosestTo(transform.position)) {
			if (IsEnemy(other.ship)) {
				target = other;
				break;
			}
		}			
		
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

		UpdateTractors();
	    UpdateWeapons();
		UpdateMovement();
	}
}
