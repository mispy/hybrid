using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class MindTask {
	public bool isStarted = false;
	public bool isFinished = false;
	public abstract void Start();
	public abstract void Update();
	public abstract void Stop();
}

public class BuildTask : MindTask {
	public readonly AIMind mind;
	public readonly Block targetBlock;

	public BuildTask(AIMind mind, Block targetBlock) {
		this.mind = mind;
		this.targetBlock = targetBlock;
	}

	public override void Start() {
		mind.crew.constructor.StartBuilding(targetBlock);
		isStarted = true;
	}

	public override void Update() {
		if (targetBlock.IsFilled)
			Stop();
	}

	public override void Stop() {
		mind.crew.constructor.StopBuilding();
		isFinished = true;
	}
}

public class AIMind : MonoBehaviour {
	private List<IntVector2> path;
	private CharacterController controller;

	public Crew crew;
	public MindTask task = null;
	public Queue<MindTask> taskQueue = new Queue<MindTask>();

	public Ship myShip;
	
	void Awake() {
		crew = GetComponentInParent<Crew>();
	}
	
	void Start() {
		myShip = Ship.allActive[1];
		//InvokeRepeating("PathToConsole", 0.0f, 1.0f);
	}

	void UpdateTasks() {
		if (task != null) {
			task.Update();
			
			if (task.isFinished)
				task = null;
		}
		
		if (task == null && taskQueue.Count > 0) {
			task = taskQueue.Dequeue();
			task.Start();
		}

		if (task == null) {
			task = FindNextTask();
			if (task != null)
				task.Start();
		}
	}

	MindTask FindNextTask() {
		foreach (var block in myShip.blueprint.blocks.All) {
			if (!block.IsFilled) {
				return new BuildTask(this, block);
			}
		}

		return null;
	}

	void Update() {
		var speed = 0.3f;
		
		if (path != null && path.Count > 0) {
			Vector3 worldPos = Ship.allActive[0].BlockToWorldPos(path[0]);
			var dist = worldPos - transform.position;
			
			if (dist.magnitude < speed) {
				transform.position = worldPos;
				path.RemoveAt(0);
			} else {
				controller.Move(dist.normalized * speed);
			}
		}

		UpdateTasks();
	}

	void PathToConsole() {
		var ship = Ship.allActive[0];

		var currentPos = ship.WorldToBlockPos(transform.position);
		var console = ship.blocks.FindType("console");

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
}
