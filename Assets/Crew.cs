using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public Block interactBlock = null;
	public Ship boardedShip = null;

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Vector2 lastMovement;

	public Block standingBlock = null;
	public IntVector2 targetBlockPos;
	public static Crew player;

	// Use this for initialization
	void Start () {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();
		player = this;
	}

	void UpdateGravity() {
		// if we're on a ship, just check if we can stay on that ship
		if (boardedShip != null && boardedShip.hasGravity) {
			var block = boardedShip.BlockAtLocalPos(transform.localPosition);
			if (block != null) {
				standingBlock = block;
				return;
			}
		}

		Block newBlock = null;

		foreach (var hit in Physics.OverlapSphere(transform.position, collider.bounds.size.x)) {
			if (hit.attachedRigidbody == null) continue;
			var ship = hit.attachedRigidbody.gameObject.GetComponent<Ship>();
			if (ship != null && hit.gameObject.layer == Block.floorLayer) {
				var block = ship.BlockAtWorldPos(hit.transform.position);
				if (block != null) {
					newBlock = block;
					break;
				}
			}
		}

		if (newBlock == null && standingBlock != null) {
			gameObject.transform.parent = null;
			rigidBody.isKinematic = false;
			standingBlock = null;
			boardedShip = null;
		} else if (newBlock != null && (standingBlock == null || newBlock.ship != standingBlock.ship)) {
			standingBlock = newBlock;
			if (standingBlock.ship.hasGravity) {
				boardedShip = standingBlock.ship;
				transform.rotation = boardedShip.transform.rotation;
				transform.parent = boardedShip.transform;
				rigidBody.isKinematic = true;
			}
		}
	}

	IEnumerator MoveToBlock() {
		Debug.Log("moving");
		var speed = 0.1f;
		var targetPos = (Vector3)boardedShip.BlockToLocalPos(targetBlockPos);

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

	// Update is called once per frame
	void Update () {
		UpdateGravity();
		if (Input.GetKeyDown(KeyCode.E) && Game.main.activeShip != null) {
			Game.main.activeShip = null;
			return;
		}

		if (boardedShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", boardedShip.rigidBody.velocity.x, boardedShip.rigidBody.velocity.y);
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

		if (boardedShip == null) {
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
		} else {
			var bp = standingBlock.pos;

			//Debug.LogFormat("{0} {1}", bp, targetBlockPos);
			if (Input.GetKey(KeyCode.W))
				bp.y += 1;
			if (Input.GetKey(KeyCode.S))
				bp.y -= 1;
			if (Input.GetKey(KeyCode.A))
				bp.x -= 1;
			if (Input.GetKey(KeyCode.D))
				bp.x += 1;

			var destBlock = boardedShip.blocks[bp];

			if (destBlock == null || destBlock.collisionLayer == Block.floorLayer) {
				if (bp != standingBlock.pos && bp != targetBlockPos) {
					targetBlockPos = bp;
					StopCoroutine("MoveToBlock");
					StartCoroutine("MoveToBlock");
				}
			}
		}

	}
}
