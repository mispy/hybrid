using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AICrew : MonoBehaviour {
	private List<IntVector2> path;
	private CharacterController controller;

	void PathToConsole() {
		var ship = Game.main.testShip;

		var currentPos = ship.WorldToBlockPos(transform.position);
		var console = ship.blocks.FindType(Block.types["console"]);

		var nearestBlock = ship.blocks[currentPos];
		if (nearestBlock == null) {
			foreach (var bp in ship.blocks.Neighbors(currentPos)) {
				if (ship.blocks[bp] != null)
					nearestBlock = ship.blocks[bp];
			}
		}

		if (nearestBlock == null) {
			// we're not next to the ship yet, just move towards it
			path = new List<IntVector2>() { console.pos };
			return;
		}

		path = ship.blocks.PathBetween(currentPos, console.pos);
	}
		
	void Awake() {
		controller = GetComponent<CharacterController>();
	}

	void Start() {
		InvokeRepeating("PathToConsole", 0.0f, 1.0f);
	}

	void Update() {
		var speed = 0.3f;

		if (path != null && path.Count > 0) {
			Vector3 worldPos = Game.main.testShip.BlockToWorldPos(path[0]);
			var dist = worldPos - transform.position;

			if (dist.magnitude < speed) {
				transform.position = worldPos;
				path.RemoveAt(0);
			} else {
				controller.Move(dist.normalized * speed);
			}
		}
	}
}
