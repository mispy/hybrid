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

    public CrewBody crew;
    public Blockform ship;
	public LineRenderer pathLine;

    void Awake() {
        crew = GetComponent<CrewBody>();
        crew.mind = this;
        pathLine = gameObject.AddComponent<LineRenderer>();
        pathLine.material = GetComponent<SpriteRenderer>().material;
        pathLine.SetWidth(0.1f, 0.1f);
        pathLine.SetColors(Color.green, Color.green);
        pathLine.sortingLayerName = "UI";
    }

    void OnEnable() {
        blockPath = new List<IntVector2>();
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
			lineSeq.Add(crew.maglockShip.BlockToWorldPos(pos));
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
	
    public Block toRepair = null;

	void Update() {
        ship = crew.maglockShip;

		DrawPath();

        if (crew.maglockShip != null) {
            UpdateLockedMovement();
        } else {
            UpdateFreeMovement();
        }

        /*foreach (var other in Crew.all) {
            if (IsEnemy(other) && Util.LineOfSight(crew.gameObject, other.transform.position)) {
                crew.weapon.Fire(other.transform.position);
            }
        }*/

		/*if (crew.currentBlock == null && crew.job == null) {
			var floor = Util.GetRandom(crew.Ship.blocks.Find("Floor").ToList());
			crew.job = new MoveJob(floor.pos);
		}*/
    }

    bool IsEnemy(CrewBody other) {
        return false;
    }

    void UpdateLockedMovement() {
        if (blockPath.Count == 0) return;

        var bp = blockPath[0];

        if (bp == crew.currentBlockPos) {
            blockPath.RemoveAt(0);
			/*var s = "";
			foreach (var pos in blockPath) {
				s += pos.ToString() + " ";
			}
			Debug.Log(s);*/
		} else if (!crew.maglockShip.blocks.IsPassable(bp)) {
			blockPath.Clear();
        } else if (bp != crew.maglockMoveBlockPos) {
            crew.maglockMoveBlockPos = bp;
        }
    }

    void UpdateFreeMovement() {
        if (blockPath.Count == 0) return;

        var speed = 10f;    
        
        Vector3 worldPos = crew.maglockShip.BlockToWorldPos(blockPath[0]);
        var dist = worldPos - transform.position;
        
        if (dist.magnitude < 1f) {
            crew.transform.position = worldPos;
            blockPath.RemoveAt(0);
            crew.rigidBody.velocity = Vector3.zero;
        } else {
            crew.rigidBody.velocity = dist.normalized * speed;
        }
    }

    public bool CanReach(IntVector2 destPos) {
		var ship = Game.playerShip;
		return BlockPather.PathExists(ship.blocks, crew.currentBlockPos, destPos);
    }

	public void PleaseMove() {
		foreach (var neighbor in IntVector2.Neighbors(crew.currentBlockPos)) {
			if (crew.maglockShip.blocks.IsPassable(neighbor)) {
				crew.maglockMoveBlockPos = neighbor;
			}
		}
	}

    public void SetMoveDestination(IntVector2 destPos) {
        if (currentDest == destPos)
            return;

        var ship = crew.maglockShip;
        var currentPos = crew.currentBlockPos;

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
