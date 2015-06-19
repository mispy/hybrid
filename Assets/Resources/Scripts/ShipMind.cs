using UnityEngine;
using System.Collections;



public class ShipMind : MonoBehaviour {
	public Ship myShip;

	// Use this for initialization
	void Start () {
		myShip = GetComponent<Ship>();
	}

	bool IsEnemy(Ship otherShip) {
		return (otherShip != myShip);
	}

	void UpdateTractors() {
		foreach (var tractor in myShip.GetBlockComponents<TractorBeam>()) {
			tractor.Stop();

			foreach (var target in tractor.GetViableTargets()) {
				if (target.CompareTag("Item")) {
					tractor.Fire(target.transform.position);
				}
			}
		}
	}

	void UpdateWeapons() {
		foreach (var launcher in myShip.GetBlockComponents<TorpedoLauncher>()) {
			var hit = launcher.GetProbableHit();
			if (hit == null) continue;

			var otherShip = hit.gameObject.GetComponentInParent<Ship>();

			if (otherShip != null && IsEnemy(otherShip)) {
				launcher.Fire();
			}
		}
	}

	void UpdateMovement() {		
		Ship targetShip = null;
		
		foreach (var otherShip in Ship.ClosestTo(transform.position)) {
			if (IsEnemy(otherShip)) {
				targetShip = otherShip;
				break;
			}
		}
		
		if (targetShip != null) {
			myShip.MoveTowards(targetShip.transform.position);
		}
	}

	// Update is called once per frame
	void Update () {
		UpdateTractors();
		UpdateWeapons();
		UpdateMovement();
	}
}
