using UnityEngine;
using UnityEngine.Networking;
using System;
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
		this.form = mind.ship;
	}

	void RecalcEngagePath() {
		if (target == null) return; 

        // We want to try and get behind them
        var weaponSide = target.GetSideWithMostWeapons();
        var destOffset = target.transform.TransformVector((Vector2)weaponSide) * target.length;

        var destination = target.transform.position + destOffset*2;
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
        if (!form.hasMindPilot) return;

		target = mind.nearestEnemy;

		if (target != null && Vector2.Distance(target.transform.position, form.transform.position) <= maxFiringRange - 10 && Util.LineOfSight(form, target)) {
			var diff = form.RotateTowards(target.transform.position, form.GetSideWithMostWeapons());
			//return;
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
		form = mind.ship;
	}

	void Start() {
		InvokeRepeating("UpdatePath", 0f, 0.5f);
	}

	void UpdatePath() {
		var nearestEdge = transform.position.normalized * Game.activeSector.radius;
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
    [HideInInspector]
    public Blockform ship;
    [ReadOnlyAttribute]
    public IEnumerable<Blockform> enemies;
    [ReadOnlyAttribute]
    public Blockform nearestEnemy;
    [ReadOnlyAttribute]
	public PoolBehaviour tactic;

    void Start () {
        ship = GetComponent<Blockform>();
    }
    
    void Update() {
        enemies = Blockform.ClosestTo(transform.position).Where((other) => IsEnemy(other));

        if (enemies.Count() > 0) {
            enemies = enemies.OrderBy((other) => -other.poweredWeapons.Count);
            nearestEnemy = enemies.First();
        }

        AvoidCollision();
        UpdateWeapons();
        UpdateTactic();
        UpdateShields();
    } 


    public void AvoidCollision() {
        if (!ship.hasMindPilot) return;

        foreach (var form in Util.ShipsInRadius(transform.position, ship.length*2)) {
            if (form == ship) continue;

            var local = transform.InverseTransformPoint(form.transform.position);
            if (local.x > 0)
                ship.FireThrusters(Facing.right);
            if (local.x < 0)
                ship.FireThrusters(Facing.left);
            if (local.y > 0)
                ship.FireThrusters(Facing.up);
            if (local.y < 0)
                ship.FireThrusters(Facing.down);
        }
    }


    bool IsEnemy(Blockform otherShip) {
        return otherShip != ship && (ship == Game.playerShip || otherShip == Game.playerShip);
    }

    void UpdateTractors() {
        foreach (var tractor in ship.GetBlockComponents<TractorBeam>()) {
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

        foreach (var console in ship.GetBlockComponents<Console>()) {
            if (console.crew == null || console.crew.mind == null) continue;

            Blockform target = null;

            foreach (var block in console.linkedBlocks) {
                var launcher = block.GetBlockComponent<ProjectileLauncher>();
                if (launcher == null) continue;

                if (target == null) {
                    foreach (var enemy in enemies) {
                        if (launcher.CanHit(enemy)) {
                            target = enemy;
                            break;
                        }
                    }
                }

                if (target == null) continue;

                var dist = Vector2.Distance(launcher.transform.position, target.transform.position);
                var timeToHit = dist / launcher.launchVelocity;
                var correction = target.rigidBody.velocity * timeToHit;
                var targetPos = target.transform.position + correction;

                launcher.turret.AimTowards(targetPos);
                if (!Util.TurretBlocked(ship, launcher.turret.TipPosition, targetPos)) {
                    launcher.OnFire();
                }
            }
        }
    }

    void UpdateShields() {
        if (ship.shields == null || !ship.shields.hasAIControl || nearestEnemy == null)
            return;

        var targetPos = ship.shields.ellipse.positions.OrderBy((pos) => Vector2.Distance(ship.shields.transform.TransformPoint(pos), nearestEnemy.transform.position)).First();
        var targetIndex = Array.IndexOf(ship.shields.ellipse.positions, targetPos);
        var targetAngle = (float)targetIndex / ship.shields.ellipse.positions.Length;

        ship.shields.angle = targetAngle - ship.shields.arcLength/2;
    }

	void SetTactic<T>() where T : PoolBehaviour {
		if (tactic is T) return;
		if (tactic != null) Destroy(tactic);
		tactic = gameObject.AddComponent<T>();
	}

	void UpdateTactic() {
		//if (!ship.hasActiveShields) {
		//	SetTactic<FleeingTactic>();
		//} else {
			SetTactic<EngageTactic>();
		//}
	}
}
