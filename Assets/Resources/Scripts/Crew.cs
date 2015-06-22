using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public static Crew player;

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Constructor constructor;



	// Crew can be maglocked to a ship even if they don't have a current block
	// bc they attach to the sides as well
	public Ship maglockShip = null;
	public IntVector2 maglockMoveBlockPos;
	public Block currentBlock = null;

	public Block controlConsole = null;


	void Awake() {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();

		var obj = Pool.For("Constructor").TakeObject();	
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.SetActive(true);
		constructor = GetComponentInChildren<Constructor>();
	}

	IEnumerator MoveToBlock() {
		var speed = 0.1f;
		var targetPos = (Vector3)maglockShip.BlockToLocalPos(maglockMoveBlockPos);

		while (true) {
			if (maglockShip == null) break;

			var pos = transform.localPosition;
			var dist = targetPos - pos;

			if (dist.magnitude > speed) {
				dist.Normalize();
				dist = dist*speed;
			}

			transform.localPosition += dist;

			if (Vector3.Distance(transform.localPosition, targetPos) < Vector3.kEpsilon) {
				break;
			}

			yield return null;
		}
	}

	void UpdateCurrentBlock() {
		currentBlock = Block.AtWorldPos(transform.position);

		if (currentBlock != controlConsole)
			controlConsole = null;
	}

	public void UseBlock(Block block)  {
		controlConsole = block;
	}

	void SetMaglock(Ship ship) {
		maglockShip = ship;
		transform.rotation = maglockShip.transform.rotation;
		transform.parent = maglockShip.transform;
		rigidBody.isKinematic = true;
	}

	void StopMaglock() {
		gameObject.transform.parent = null;
		rigidBody.isKinematic = false;
		maglockShip = null;
	}
	
	bool CanMaglock(Ship ship) {
		var blockPos = ship.WorldToBlockPos(transform.position);

		if (ship.blocks[blockPos] != null)
			return true;

		foreach (var bp in ship.blocks.Neighbors(blockPos)) {
			if (ship.blocks[bp] != null)
				return true;
		}

		return false;
	}

	void UpdateMaglock() {
		if (maglockShip != null && CanMaglock(maglockShip))
			return;

		foreach (var ship in Ship.allActive) {
			if (CanMaglock(ship)) {
				SetMaglock(ship);
				return;
			}
		}

		// nothing to maglock
		StopMaglock();
	}

	void Update() {
		UpdateCurrentBlock();
		UpdateMaglock();
	}
}
