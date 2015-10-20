using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EngageTactic : PoolBehaviour {
	ShipMind mind;
	Blockform target;
	List<Vector2> path = new List<Vector2>();
	Blockform form;
	float maxFiringRange = 100f;

	void Awake() {
		this.mind = GetComponent<ShipMind>();
		this.form = mind.form;
	}

	void RecalcEngagePath() {
		if (target == null) return; 

		var destination = target.transform.position;
		var dir = destination - form.transform.position;
		var offset = target.length;// + maxFiringRange;

		//form.pather.transform.rotation = Quaternion.LookRotation(Vector3.forward, -dir);
		foreach (var cardinal in form.pather.Cardinals().Reverse()) {
			var candidate = (Vector2)destination + cardinal*offset*2;// + (Vector2)target.rigidBody.velocity;

			if (form.pather.IsPassable(candidate)) {
				destination = candidate;
				break;
			}
		}

		path = form.pather.PathFromNose(destination);
	}

	void Start() {
		InvokeRepeating("RecalcEngagePath", 0f, 0.5f);
	}
	
	void Update() {		
		target = mind.nearestEnemy;

		if (target != null && Vector2.Distance(target.transform.position, form.transform.position) <= maxFiringRange - 10 && Util.LineOfSight(form, target)) {
			var diff = form.RotateTowards(target.transform.position, form.GetSideWithMostWeapons());
			return;
		}

		//if (path != null && path.Count > 0 && form.BlocksAtWorldPos(path[0]).Count() != 0)
		//	path.RemoveAt(0);
		
		if (path != null && path.Count == 0)
			path = null;
		
		if (path != null) {
			DebugUtil.DrawPath(path);
			form.FollowPath(path);
		}
	}
}

public class FleeingTactic : PoolBehaviour {
	Blockform form;
	List<Vector2> path;

	void Awake() {
		var mind = GetComponent<ShipMind>();
		form = mind.form;
	}

	void Start() {
		InvokeRepeating("UpdatePath", 0f, 0.5f);
	}

	void UpdatePath() {
		var nearestEdge = transform.position.normalized * Game.activeSector.sector.radius;
		path = form.pather.PathFromNose(nearestEdge);

		if (form.canFoldJump) {
			//form.FoldJump();
		}
	}

	void Update() {
		if (path != null)
			form.FollowPath(path);
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
        return ship.DispositionTowards(otherShip) == Disposition.hostile;
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

        foreach (var launcher in form.GetBlockComponents<ProjectileLauncher>()) {
            launcher.turret.AimTowards(nearestEnemy.transform.position);

            var hit = launcher.GetProbableHit(100f);
            if (hit == null) continue;

            var otherShip = hit.attachedRigidbody.GetComponent<Blockform>();

            if (otherShip != null && IsEnemy(otherShip.ship)) {
                launcher.Fire();
            }
        }
    }

	void SetTactic<T>() where T : PoolBehaviour {
		if (tactic is T) return;
		if (tactic != null) Destroy(tactic);
		tactic = gameObject.AddComponent<T>();
	}

	void UpdateTactic() {
		if (!form.hasActiveShields) {
			SetTactic<FleeingTactic>();
		} else {
			SetTactic<EngageTactic>();
		}
	}

    // Update is called once per frame
    void Update () {
        if (form.maglockedCrew.Count == 0 || form.ship == Game.playerShip) return;

        var enemies = Blockform.ClosestTo(transform.position).Where((other) => IsEnemy(other.ship));
        
        if (enemies.Count() > 0) {
            enemies = enemies.OrderBy((other) => -other.poweredWeapons.Count);
            nearestEnemy = enemies.First();
        }

        UpdateTractors();
        UpdateWeapons();
		UpdateTactic();
   } 
}
