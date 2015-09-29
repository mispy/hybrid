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
    
    void Awake() {
        blockPath = new List<IntVector2>();
        body = GetComponentInParent<CrewBody>();
        crew = GetComponentInParent<CrewBody>().crew;
        crew.mind = this;
    }

    void Update() {
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
    }

    bool IsEnemy(Crew other) {
        return other != this.body.crew;
    }

    void UpdateLockedMovement() {
        if (blockPath.Count == 0) return;

        var bp = blockPath[0];

        if (bp == body.currentBlockPos) {
            blockPath.RemoveAt(0);
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
		if (!ship.blocks.IsPassable(destPos))
			return false;
		if (!ship.blocks.IsPassable(crew.body.currentBlockPos))
			return false;
        return BlockPather.PathBetween(ship.blocks, destPos, crew.body.currentBlockPos) != null;
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

        var lineSeq = new List<Vector2>();
        lineSeq.Add(transform.position);
        foreach (var pos in blockPath) {
            lineSeq.Add(ship.form.BlockToWorldPos(pos));
        }
        
        for (var i = 1; i < lineSeq.Count; i++) {
            Debug.DrawLine(lineSeq[i-1], lineSeq[i], Color.green);
        }
    }
}
