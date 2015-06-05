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


	public Ship currentShip = null;
	public Block currentBlock = null;
	public bool isGravityLocked = false;

	public IntVector2 targetBlockPos;

	// Use this for initialization
	void Start () {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();
		player = this;
	}

	IEnumerator MoveToBlock() {
		Debug.Log("moving");
		var speed = 0.1f;
		var targetPos = (Vector3)currentShip.BlockToLocalPos(targetBlockPos);

		while (true) {
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
		// if we're on a ship, just check if we can stay on that ship
		if (currentShip != null) {
			var block = currentShip.BlockAtWorldPos(transform.position);

			if (block != null) {
				currentBlock = block;
			} else {
				currentBlock = null;
				OnShipLeave(currentShip);
			}
		} else {
			currentBlock = Block.AtWorldPos(transform.position);
			if (currentBlock != null) {
				OnShipEnter(currentBlock.ship);
			}
		}		
	}

	void OnShipEnter(Ship ship) {
		Debug.Log("entering ship");
		currentShip = ship;
	}

	void OnShipLeave(Ship ship) {
		Debug.Log("leaving ship");
		currentShip = null;
	}

	void UpdateGravityLock() {
		var hasGravity = currentShip != null && currentShip.hasGravity;
		if (hasGravity && !isGravityLocked) {
			transform.rotation = currentShip.transform.rotation;
			transform.parent = currentShip.transform;
			rigidBody.isKinematic = true;
			isGravityLocked = true;
		} else if (!hasGravity && isGravityLocked) {
			gameObject.transform.parent = null;
			rigidBody.isKinematic = false;
			isGravityLocked = false;
		}
	}

	void HandleLockedMovement() {
		var bp = currentBlock.pos;
		
		//Debug.LogFormat("{0} {1}", bp, targetBlockPos);
		if (Input.GetKey(KeyCode.W))
			bp.y += 1;
		if (Input.GetKey(KeyCode.S))
			bp.y -= 1;
		if (Input.GetKey(KeyCode.A))
			bp.x -= 1;
		if (Input.GetKey(KeyCode.D))
			bp.x += 1;
		
		var destBlock = currentShip.blocks[bp];
		
		if (destBlock == null || destBlock.collisionLayer == Block.floorLayer) {
			if (bp != currentBlock.pos && bp != targetBlockPos) {
				targetBlockPos = bp;
				StopCoroutine("MoveToBlock");
				StartCoroutine("MoveToBlock");
			}
		}
	}

	void HandleFreeMovement() {
		var speed = 60f * Time.deltaTime;
		Vector2 vel = rigidBody.velocity;
		if (Input.GetKey(KeyCode.W)) {
			vel += (Vector2)transform.up * speed;
		}
		
		if (Input.GetKey(KeyCode.A)) {
			vel += -(Vector2)transform.right * speed;
		}
		
		if (Input.GetKey(KeyCode.D)) {
			vel += (Vector2)transform.right * speed;
		}
		
		if (Input.GetKey(KeyCode.S)) {
			vel += -(Vector2)transform.up * speed; 
		}
		
		rigidBody.velocity = vel;
	}

	public bool designing;

	void StartDesigning() {
	}

	void StopDesigning() {
	}

	// Update is called once per frame
	void Update () {
		UpdateCurrentBlock();
		UpdateGravityLock();

		if (Input.GetKeyDown(KeyCode.E) && Game.main.activeShip != null) {
			Game.main.activeShip = null;
			return;
		}

		if (currentShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
		}
		
		if (Game.main.activeShip != null) return;				

		if (Input.GetKeyDown(KeyCode.E) && interactBlock != null) {
			Game.main.activeShip = interactBlock.ship;
			return;
		}

		
		if (interactBlock != null) {
			//interactBlock.GetComponent<SpriteRenderer>().color = Color.white;
		}
		interactBlock = null;

		var nearbyBlocks = Block.FindInRadius(transform.position, Block.worldSize*1f);
		foreach (var block in nearbyBlocks) {
			if (block.type == Block.types["console"]) {
				interactBlock = block;
				//interactBlock.GetComponent<SpriteRenderer>().color = Color.yellow;
			}
		}


		if (isGravityLocked) {
			HandleLockedMovement();
		} else {
			HandleFreeMovement();
		}	
	}
}
