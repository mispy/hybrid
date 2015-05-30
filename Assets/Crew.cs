using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public Block interactBlock = null;
	public Ship boardedShip = null;

	public Rigidbody2D rigid;
	public BoxCollider2D collider;

	public Vector2 lastMovement;

	public Block standingBlock = null;

	// Use this for initialization
	void Start () {
		collider = GetComponent<BoxCollider2D>();
		rigid = GetComponent<Rigidbody2D>();
	}

	void UpdateGravity() {
		Block newBlock = null;

		// if we're on a ship, just check if we can stay on that ship
		if (boardedShip != null && boardedShip.blocks[boardedShip.WorldToBlockPos(transform.position)] != null)
			return;

		foreach (var hit in Physics2D.OverlapAreaAll((Vector2)collider.bounds.min, (Vector2)collider.bounds.max)) {
			var ship = hit.attachedRigidbody.gameObject.GetComponent<Ship>();
			Debug.LogFormat("{0} {1} {2}", ship, hit.gameObject.layer, Block.floorLayer);
			if (ship != null && hit.gameObject.layer == Block.floorLayer) {
				var block = ship.blocks[ship.WorldToBlockPos(hit.transform.position)];
				if (block != null) {
					newBlock = block;
					break;
				}
			}
		}

		if (newBlock == null && standingBlock != null) {
			gameObject.transform.parent = null;
			rigid.isKinematic = false;
			standingBlock = null;
			boardedShip = null;
		} else if (newBlock != null && (standingBlock == null || newBlock.ship != standingBlock.ship)) {
			standingBlock = newBlock;
			boardedShip = standingBlock.ship;
			transform.rotation = boardedShip.transform.rotation;
			transform.parent = boardedShip.transform;
			rigid.isKinematic = true;
		}
					
		
		if (boardedShip != null) {
			Debug.Log("on ship");
		} else {
			Debug.Log("off ship");
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateGravity();
	
		if (Input.GetKeyDown(KeyCode.E) && Game.main.activeShip != null) {
			rigid.isKinematic = false;
			transform.parent = null;
			Game.main.activeShip = null;
			return;
		}

		if (Game.main.activeShip != null) return;

		if (Input.GetKeyDown(KeyCode.E) && interactBlock != null) {
			Game.main.activeShip = interactBlock.ship;
			rigid.isKinematic = true;
			transform.rotation = Game.main.activeShip.transform.rotation;
			transform.parent = Game.main.activeShip.gameObject.transform;
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
			Vector2 vel = rigid.velocity;
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
			
			rigid.velocity = vel;
		} else {
			Vector2 offset = new Vector2(0.0f, 0.0f);
			if (Input.GetKey(KeyCode.W))
				offset += new Vector2(0.0f, 0.1f);
			if (Input.GetKey(KeyCode.S))
				offset += new Vector2(0.0f, -0.1f);
			if (Input.GetKey(KeyCode.A))
				offset += new Vector2(-0.1f, 0.0f);
			if (Input.GetKey(KeyCode.D))
				offset += new Vector2(0.1f, 0.0f);

			foreach (var hit in Physics2D.OverlapAreaAll((Vector2)collider.bounds.min+offset, (Vector2)collider.bounds.max+offset)) {
				if (hit.gameObject != gameObject && hit.gameObject.layer != Block.floorLayer) {
					return;
				}
			}

			transform.Translate(offset);
			lastMovement = offset;
		}

	}
}
