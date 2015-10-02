using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipTactic {
	public ShipMind mind;
	public virtual void Update() { }
}

public class EngageTactic : ShipTactic {
	List<Vector2> path = new List<Vector2>();
	Blockform form;
	float maxFiringRange = 20f;

	public EngageTactic(ShipMind mind) {
		this.mind = mind;
		this.form = mind.form;
	}

	void RecalcEngagePath() {
		Blockform target = mind.nearestEnemy;
		if (target == null) return; 

		// Find the nearest part of the enemy ship to us
		var destination = target.transform.position;
		var targetVec = destination - form.transform.position;
		var targetDir = targetVec.normalized;
		var targetHits = Physics.SphereCastAll(form.transform.position, form.width, targetDir, targetVec.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
		foreach (var hit in targetHits) {
			if (hit.rigidbody == target.rigidBody) {
				destination = hit.point;
			}
		}

		// We want to leave some distance between us and the enemy
		destination = destination - (targetDir * (form.height + maxFiringRange));

		path = form.pather.PathBetween(form.transform.position, destination);
	}
	
	public override void Update() {  
		if (path != null && path.Count > 0 && form.BlocksAtWorldPos(path[0]).Count() != 0)
			path.RemoveAt(0);

		if (path != null && path.Count == 0)
			path = null;

		if (path != null) {
			DebugUtil.DrawPath(path);
			form.FollowPath(path);
		}

		RecalcEngagePath();
	}
}

public class ShipMind : PoolBehaviour {
    public Ship ship;
    public Blockform form;
    public Blockform nearestEnemy;
	public ShipTactic tactic;

    // Use this for initialization
    void Start () {
        form = GetComponent<Blockform>();
        ship = GetComponent<Blockform>().ship;
		tactic = new EngageTactic(this);
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
            launcher.AimTowards(nearestEnemy.transform.position);

            var hit = launcher.GetProbableHit();
            if (hit == null) continue;

            var otherShip = hit.gameObject.GetComponentInParent<Blockform>();

            if (otherShip != null && IsEnemy(otherShip.ship)) {
                launcher.Fire();
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (form.maglockedCrew.Count == 0 || form.ship == Game.playerShip) return;
        
        foreach (var other in Blockform.ClosestTo(transform.position)) {
            if (IsEnemy(other.ship)) {
                nearestEnemy = other;
                break;
            }
        }            

        UpdateTractors();
        UpdateWeapons();

		tactic.Update();
   } 
}
