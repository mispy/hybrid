using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CrewMind : MonoBehaviour {
    public List<IntVector2> blockPath { get; private set; }
    public IntVector2? currentDest {
        get {
            if (blockPath.Count == 0)
                return null;

            return blockPath.Last();
        }
    }
    private CharacterController controller;

    public CrewBody body;

    public Crew crew;
    
	public LineRenderer pathLine;

    void Awake() {
        blockPath = new List<IntVector2>();
        body = GetComponentInParent<CrewBody>();
        crew = GetComponentInParent<CrewBody>().crew;
        crew.mind = this;
		pathLine = GetComponent<LineRenderer>();
		pathLine.sortingLayerName = "UI";
	}

	void DrawPath() {
		if (blockPath.Count == 0) {
			pathLine.enabled = false;
			return;
		}

		pathLine.enabled = true;

		var lineSeq = new List<Vector2>();
		lineSeq.Add(transform.position);
		foreach (var pos in blockPath) {
			lineSeq.Add(crew.body.maglockShip.BlockToWorldPos(pos));
		}

		pathLine.SetVertexCount(lineSeq.Count*2);
		for (var i = 0; i < lineSeq.Count; i++) {
			pathLine.SetPosition(i, lineSeq[i]);
		}
		
		// HACK (Mispy): See http://forum.unity3d.com/threads/line-renderer-problem.21406/
		for (var i = 0; i < lineSeq.Count; i++) {
			pathLine.SetPosition(lineSeq.Count+i, lineSeq[lineSeq.Count-1-i]);
		}
		/*var s = "";
		foreach (var pos in lineSeq) {
			s += pos.ToString() + " ";
		}
		Debug.Log(s);*/
		
	}
	
	void Update() {
		DrawPath();

        if (body.maglockShip != null) {
            UpdateLockedMovement();
        } else {
            UpdateFreeMovement();
        }
        /*foreach (var other in Crew.all) {
            if (IsEnemy(other) && Util.LineOfSight(body.gameObject, other.transform.position)) {
                body.weapon.Fire(other.transform.position);
            }
        }*/

		/*if (body.currentBlock == null && crew.job == null) {
			var floor = Util.GetRandom(crew.Ship.blocks.Find("Floor").ToList());
			crew.job = new MoveJob(floor.pos);
		}*/
    }

    bool IsEnemy(Crew other) {
        return other != this.body.crew;
    }

    void UpdateLockedMovement() {
        if (blockPath.Count == 0) return;

        var bp = blockPath[0];

        if (bp == body.currentBlockPos) {
            blockPath.RemoveAt(0);
			/*var s = "";
			foreach (var pos in blockPath) {
				s += pos.ToString() + " ";
			}
			Debug.Log(s);*/
		} else if (!crew.body.maglockShip.blocks.IsPassable(bp)) {
			blockPath.Clear();
        } else if (bp != body.maglockMoveBlockPos) {
            body.maglockMoveBlockPos = bp;
        }
    }

    void UpdateFreeMovement() {
        if (blockPath.Count == 0) return;

        var speed = 10f;    
        
        Vector3 worldPos = crew.Ship.form.BlockToWorldPos(blockPath[0]);
        var dist = worldPos - transform.position;
        
        if (dist.magnitude < 1f) {
            body.transform.position = worldPos;
            blockPath.RemoveAt(0);
            body.rigidBody.velocity = Vector3.zero;
        } else {
            body.rigidBody.velocity = dist.normalized * speed;
        }
    }

    public bool CanReach(IntVector2 destPos) {
		var ship = Game.playerShip;
		return BlockPather.PathExists(ship.blocks, crew.body.currentBlockPos, destPos);
    }

	public void PleaseMove() {
		foreach (var neighbor in IntVector2.Neighbors(crew.body.currentBlockPos)) {
			if (crew.body.maglockShip.blocks.IsPassable(neighbor)) {
				crew.job = null;
				crew.body.maglockMoveBlockPos = neighbor;
			}
		}
	}

    public void SetMoveDestination(IntVector2 destPos) {
        if (currentDest == destPos)
            return;

        var ship = body.maglockShip.ship;
        var currentPos = crew.body.currentBlockPos;

        var nearestBlock = ship.blocks[currentPos, BlockLayer.Base];
        if (nearestBlock == null) {
            foreach (var bp in IntVector2.Neighbors(currentPos)) {
                if (ship.blocks[bp, BlockLayer.Base] != null)
                    nearestBlock = ship.blocks[bp, BlockLayer.Base];
            }
        }

        if (nearestBlock == null) {
            // we're not next to the ship yet, just move towards it
            blockPath = new List<IntVector2>() { destPos };
        } else {
            var path = BlockPather.PathBetween(ship.blocks, currentPos, destPos);
            if (path != null) blockPath = path;
        }
    }
}
