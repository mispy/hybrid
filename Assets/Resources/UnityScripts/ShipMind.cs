using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EngageTactic : PoolBehaviour {
	ShipMind mind;
	Blockform target;
	List<Vector2> path = new List<Vector2>();
	Blockform form;
	float maxFiringRange = 50f;

	void Awake() {
		this.mind = GetComponent<ShipMind>();
		this.form = mind.form;
	}

	void RecalcEngagePath() {
		if (target == null) return; 

		var destination = target.transform.position;
		var dir = destination - form.transform.position;
		var offset = form.height + maxFiringRange;

		//form.pather.transform.rotation = Quaternion.LookRotation(Vector3.forward, -dir);
		foreach (var cardinal in form.pather.Cardinals().Reverse()) {
			var candidate = (Vector2)destination + cardinal*offset + (Vector2)target.rigidBody.velocity;

			if (form.pather.IsPassable(candidate)) {
				destination = candidate;
				break;
			}
		}

		path = form.pather.PathBetween(form.transform.position + form.transform.TransformVector(new Vector2(0, form.blocks.maxY)), destination);
	}

	void Start() {
		InvokeRepeating("RecalcEngagePath", 0f, 0.5f);
	}
	
	void Update() {		
		target = mind.nearestEnemy;

		if (target != null && Vector2.Distance(target.transform.position, form.transform.position) <= maxFiringRange - 10) {
			var diff = form.RotateTowards(target.transform.position, form.GetSideWithMostWeapons());
			if (diff < 5)
				form.FireThrusters(Orientation.down);
			return;
		}

		if (path != null && path.Count > 0 && form.BlocksAtWorldPos(path[0]).Count() != 0)
			path.RemoveAt(0);
		
		if (path != null && path.Count == 0)
			path = null;
		
		if (path != null) {
			DebugUtil.DrawPath(path);
			form.FollowPath(path);
		}
	}
}

public class ShipMind : PoolBehaviour {
    public Ship ship;
    public Blockform form;
    public Blockform nearestEnemy;
	public PoolBehaviour tactic;

    // Use this for initialization
    void Start () {
        form = GetComponent<Blockform>();
        ship = GetComponent<Blockform>().ship;
		tactic = gameObject.AddComponent<EngageTactic>();
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
   } 
}
