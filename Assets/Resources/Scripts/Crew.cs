using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public static Crew player;

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Ship boardedShip = null;
	public Block currentBlock = null;
	public bool isGravityLocked = false;
	public Constructor constructor;

	public Block controlConsole = null;

	public IntVector2 targetBlockPos;

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
		var targetPos = (Vector3)boardedShip.BlockToLocalPos(targetBlockPos);

		while (true) {
			if (!isGravityLocked) break;

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
		if (currentBlock != null && currentBlock.ship != boardedShip) {
			if (boardedShip != null) OnShipLeave(boardedShip);
			OnShipEnter(currentBlock.ship);
		}
	}

	public void UseBlock(Block block)  {
		controlConsole = block;
	}

	void OnShipEnter(Ship ship) {
		Debug.Log("entering ship");
		boardedShip = ship;
	}

	void OnShipLeave(Ship ship) {
		Debug.Log("leaving ship");
		boardedShip = null;
	}

	void UpdateGravityLock() {
		var hasGravity = currentBlock != null && boardedShip != null && boardedShip.hasGravity;
		if (hasGravity && !isGravityLocked) {
			transform.rotation = boardedShip.transform.rotation;
			transform.parent = boardedShip.transform;
			rigidBody.isKinematic = true;
			isGravityLocked = true;
		} else if (!hasGravity && isGravityLocked) {
			gameObject.transform.parent = null;
			rigidBody.isKinematic = false;
			isGravityLocked = false;
		}
	}

	void Update() {
		UpdateCurrentBlock();
		UpdateGravityLock();
	}
}
