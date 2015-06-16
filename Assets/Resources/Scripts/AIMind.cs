using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MindTask {
	public bool isFinished = false;
	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void Stop() { isFinished = true; }
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

public class AttachTask : MindTask {
	public readonly AIMind mind;
	public readonly Block targetBlock;

	public AttachTask(AIMind mind, Block targetBlock) {
		this.mind = mind;
		this.targetBlock = targetBlock;
	}

	public override void Update() {
		if (mind.crew.currentBlock == targetBlock) {
			Stop();
		} else {
			mind.PathToBlock(targetBlock);
		}
	}
}

public class AIMind : MonoBehaviour {
	private List<IntVector2> blockPath = new List<IntVector2>();
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
		InvokeRepeating("UpdateTasks", 0.0f, 0.2f);
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

		foreach (var block in myShip.blueprint.blocks.All) {
			if (block.type.name == "console") {
				return new AttachTask(this, block);
			}
		}

		return null;
	}


	void Update() {
		if (crew.isGravityLocked) {
			UpdateLockedMovement();
		} else {
			UpdateFreeMovement();
		}
	}

	void UpdateLockedMovement() {		
		if (blockPath.Count == 0) return;

		var bp = blockPath[0];

		if (bp == crew.currentBlock.pos) {
			blockPath.RemoveAt(0);
		} else if (bp != crew.targetBlockPos) {
			crew.targetBlockPos = bp;
			crew.StopCoroutine("MoveToBlock");
			crew.StartCoroutine("MoveToBlock");
		}
	}

	void UpdateFreeMovement() {
		if (blockPath.Count == 0) return;

		var speed = 10f;	
		
		Vector3 worldPos = myShip.BlockToWorldPos(blockPath[0]);
		var dist = worldPos - transform.position;
		
		Debug.Log(dist.magnitude);
		
		if (dist.magnitude < 1f) {
			crew.transform.position = worldPos;
			blockPath.RemoveAt(0);
			crew.rigidBody.velocity = Vector3.zero;
		} else {
			crew.rigidBody.velocity = dist.normalized * speed;
		}
	}

	public void PathToBlock(Block block) {
		var ship = block.ship;
		var currentPos = ship.WorldToBlockPos(transform.position);

		var nearestBlock = ship.blocks[currentPos];
		if (nearestBlock == null) {
			foreach (var bp in ship.blocks.Neighbors(currentPos)) {
				if (ship.blocks[bp] != null)
					nearestBlock = ship.blocks[bp];
			}
		}

		if (nearestBlock == null) {
			// we're not next to the ship yet, just move towards it
			blockPath = new List<IntVector2>() { block.pos };
		} else {
			blockPath = ship.blocks.PathBetween(currentPos, block.pos);
		}

		var lineSeq = new List<Vector2>();
		lineSeq.Add(transform.position);
		foreach (var pos in blockPath) {
			lineSeq.Add(ship.BlockToWorldPos(pos));
		}
		
		for (var i = 1; i < lineSeq.Count; i++) {
			Debug.DrawLine(lineSeq[i-1], lineSeq[i], Color.green);
		}
	}
}
