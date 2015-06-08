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

	public IntVector2 targetBlockPos;

	public Designer designer;
	public bool isDesigning;

	void Awake() {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();
		designer = GetComponent<Designer>();
		player = this;
		this.name = "Player";
	}
		
	// Use this for initialization
	void Start () {
	}

	IEnumerator MoveToBlock() {
		Debug.Log("moving");
		var speed = 0.1f;
		var targetPos = (Vector3)linkedShip.BlockToLocalPos(targetBlockPos);

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
		currentBlock = Block.AtWorldPos(transform.position);
		if (currentBlock != null && currentBlock.ship != linkedShip) {
			if (linkedShip != null) OnShipLeave(linkedShip);
			OnShipEnter(currentBlock.ship);
		}
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
		
		var destBlock = linkedShip.blocks[bp];
		
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

	void HandleShipInput() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		var rigid = controlShip.rigidBody;	
		
		if (Input.GetKey(KeyCode.W)) {
			controlShip.FireThrusters(Orientation.up);		
		}
		
		if (Input.GetKey(KeyCode.S)) {
			controlShip.FireThrusters(Orientation.down);
		}
		
		if (Input.GetKey(KeyCode.A)) {
			//rigid.AddTorque(0.1f);
			controlShip.FireThrusters(Orientation.right);
		}
		
		if (Input.GetKey(KeyCode.D)) {
			//rigid.AddTorque(-0.1f);
			controlShip.FireThrusters(Orientation.left);
		}
		
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Log("firing???");
			foreach (var launcher in controlShip.GetBlockComponents<MissileLauncher>()) {
				Debug.Log("firing?");
				launcher.Fire(pz);
			}
		}
		
		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
		}
		
		if (Input.GetMouseButton(1)) {
			controlShip.StartTractorBeam(pz);
		} else {
			controlShip.StopTractorBeam();
		}
	}

	// Update is called once per frame
	void Update () {
		UpdateCurrentBlock();
		UpdateGravityLock();

		if (Input.GetKeyDown(KeyCode.F1)) {
			if (!designer.enabled)
				designer.enabled = true;
			else {
				designer.enabled = false;
			}
		}

		if (!designer.enabled) {
			Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

			if (Input.GetMouseButton(0)) {
				var ship = Ship.AtWorldPos(pz);

				if (ship != null) {
					var blockPos = ship.WorldToBlockPos(pz);
					var blueBlock = ship.blueprint.blocks[blockPos];
					if (blueBlock == null) {
						ship.blocks[blockPos] = null;
					} else {
						ship.blocks[blockPos] = new Block(ship.blueprint.blocks[blockPos]);
					}
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.E) && controlShip != null) {
			controlShip = null;
			return;
		}

		/*if (currentShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
		}*/
		
		if (Input.GetKeyDown(KeyCode.E) && interactBlock != null) {
			controlShip = interactBlock.ship;
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

		if (controlShip != null) {
			HandleShipInput();
		} else {
			if (isGravityLocked) {
				HandleLockedMovement();
			} else {
				HandleFreeMovement();
			}	
		}
	}
}
