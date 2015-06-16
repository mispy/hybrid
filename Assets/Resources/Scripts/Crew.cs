using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public static Crew player;
	public Block interactBlock = null;

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Ship controlShip = null;
	public Ship linkedShip = null;
	public Block currentBlock = null;
	public bool isGravityLocked = false;
	public Constructor constructor;

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
		var targetPos = (Vector3)linkedShip.BlockToLocalPos(targetBlockPos);

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
		if (currentBlock != null && currentBlock.ship != linkedShip) {
			if (linkedShip != null) OnShipLeave(linkedShip);
			OnShipEnter(currentBlock.ship);
		}
	}

	public void SitAtConsole(Block block)  {
		controlShip = block.ship;
	}

	void OnShipEnter(Ship ship) {
		Debug.Log("entering ship");
		linkedShip = ship;
	}

	void OnShipLeave(Ship ship) {
		Debug.Log("leaving ship");
		linkedShip = null;
	}

	void UpdateGravityLock() {
		var hasGravity = currentBlock != null && linkedShip != null && linkedShip.hasGravity;
		if (hasGravity && !isGravityLocked) {
			transform.rotation = linkedShip.transform.rotation;
			transform.parent = linkedShip.transform;
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
