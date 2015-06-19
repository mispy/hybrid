using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerInput : MonoBehaviour {
	public Crew crew;
	public Designer designer;

	void Awake() {
		crew = GetComponentInParent<Crew>();
		designer = gameObject.AddComponent<Designer>();
		Crew.player = crew;
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
		
		var destBlock = crew.boardedShip.blocks[bp];
		
		if (destBlock == null || destBlock.CollisionLayer == Block.floorLayer) {
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
		var ship = crew.controlConsole.ship;

		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		
		var rigid = ship.rigidBody;	
		
		if (Input.GetKey(KeyCode.W)) {
			ship.FireThrusters(Orientation.down);		
		}
		
		if (Input.GetKey(KeyCode.S)) {
			ship.FireThrusters(Orientation.up);
		}
		
		if (Input.GetKey(KeyCode.A)) {
			ship.FireAttitudeThrusters(Orientation.right);
		}
		
		if (Input.GetKey(KeyCode.D)) {
			ship.FireAttitudeThrusters(Orientation.left);
		}
		
		if (Input.GetMouseButton(0)) {
			foreach (var launcher in ship.GetBlockComponents<TorpedoLauncher>()) {
				launcher.Fire();
			}
		}
		
		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
		}
		
		if (Input.GetMouseButton(1)) {
			ship.StartTractorBeam(pz);
		} else {
			ship.StopTractorBeam();
		}
		
		
		/*if (currentShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
		}*/
	}

	void HandleCrewInput() {		
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		if (Input.GetKeyDown(KeyCode.F1)) {
			if (!designer.enabled)
				designer.StartDesigning();
			else {
				designer.StopDesigning();
			}
		}

		if (!designer.enabled) {
			if (Input.GetMouseButton(0)) {
				crew.constructor.Build(pz);
			} else {
				crew.constructor.StopBuilding();
			}
		}
		
		if (Input.GetKeyDown(KeyCode.E) && crew.currentBlock != null) {
			crew.UseBlock(crew.currentBlock);
			return;
		}	

		if (crew.isGravityLocked) {
			HandleLockedMovement();
		} else {
			HandleFreeMovement();
		}		
	}
	
	// Update is called once per frame
	void Update () {
		if (crew.controlConsole != null) {
			if (Input.GetKeyDown(KeyCode.E)) {
				crew.controlConsole = null;
			} else {
				HandleShipInput();
			}
		} else {
			HandleCrewInput();
		}
	}
}