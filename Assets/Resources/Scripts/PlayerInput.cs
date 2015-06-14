using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerInput : MonoBehaviour {
	public Crew crew;
	public Designer designer;

	void Awake() {
		crew = GetComponent<Crew>();
		designer = gameObject.AddComponent<Designer>();
		Crew.player = crew;
		this.name = "Player";
	}

	void HandleLockedMovement() {
		var bp = crew.currentBlock.pos;
		
		//Debug.LogFormat("{0} {1}", bp, targetBlockPos);
		if (Input.GetKey(KeyCode.W))
			bp.y += 1;
		if (Input.GetKey(KeyCode.S))
			bp.y -= 1;
		if (Input.GetKey(KeyCode.A))
			bp.x -= 1;
		if (Input.GetKey(KeyCode.D))
			bp.x += 1;
		
		var destBlock = crew.linkedShip.blocks[bp];
		
		if (destBlock == null || destBlock.collisionLayer == Block.floorLayer) {
			if (bp != crew.currentBlock.pos && bp != crew.targetBlockPos) {
				crew.targetBlockPos = bp;
				crew.StopCoroutine("MoveToBlock");
				crew.StartCoroutine("MoveToBlock");
			}
		}
	}
	
	void HandleFreeMovement() {
		var speed = 60f * Time.deltaTime;
		Vector2 vel = crew.rigidBody.velocity;
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
		
		crew.rigidBody.velocity = vel;
	}
	
	void HandleShipInput() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		
		var rigid = crew.controlShip.rigidBody;	
		
		if (Input.GetKey(KeyCode.W)) {
			crew.controlShip.FireThrusters(Orientation.up);		
		}
		
		if (Input.GetKey(KeyCode.S)) {
			crew.controlShip.FireThrusters(Orientation.down);
		}
		
		if (Input.GetKey(KeyCode.A)) {
			//rigid.AddTorque(0.1f);
			crew.controlShip.FireThrusters(Orientation.right);
		}
		
		if (Input.GetKey(KeyCode.D)) {
			//rigid.AddTorque(-0.1f);
			crew.controlShip.FireThrusters(Orientation.left);
		}
		
		if (Input.GetMouseButton(0)) {
			foreach (var launcher in crew.controlShip.GetBlockComponents<MissileLauncher>()) {
				launcher.Fire(pz);
			}
		}
		
		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
		}
		
		if (Input.GetMouseButton(1)) {
			crew.controlShip.StartTractorBeam(pz);
		} else {
			crew.controlShip.StopTractorBeam();
		}
		
		
		/*if (currentShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
		}*/
	}
	
	void HandleCrewInput() {		
		if (Input.GetKeyDown(KeyCode.F1)) {
			if (!designer.enabled)
				designer.StartDesigning();
			else {
				designer.StopDesigning();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.E) && crew.controlShip != null) {
			crew.controlShip = null;
			return;
		}
		
		if (!designer.enabled) {
			if (Input.GetMouseButton(0)) {
				crew.constructor.StartBuilding();
			} else {
				crew.constructor.StopBuilding();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.E) && crew.interactBlock != null) {
			crew.controlShip = crew.interactBlock.ship;
			return;
		}	
		
		if (crew.interactBlock != null) {
			//interactBlock.GetComponent<SpriteRenderer>().color = Color.white;
		}
		crew.interactBlock = null;
		
		var nearbyBlocks = Block.FindInRadius(transform.position, Block.worldSize*1f);
		foreach (var block in nearbyBlocks) {
			if (block.type == Block.types["console"]) {
				crew.interactBlock = block;
				//interactBlock.GetComponent<SpriteRenderer>().color = Color.yellow;
			}
		}
		
		if (crew.isGravityLocked) {
			HandleLockedMovement();
		} else {
			HandleFreeMovement();
		}		
	}
	
	// Update is called once per frame
	void Update () {
		if (crew.controlShip != null) {
			HandleShipInput();
		} else {
			HandleCrewInput();
		}
	}
}