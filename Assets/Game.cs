﻿using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
	public static Game main;

	public GameObject shipPrefab;

	public GameObject tractorBeam;
	public GameObject currentBeam;
	public ParticleSystem thrust;

	private List<Block> placedBlocks = new List<Block>();

	public Crew player;

	public GameObject boxColliderPrefab;

	public Ship activeShip = null;

	private Block adjoiningBlock = null;
	private Ship adjoiningShip = null;

	// this "ship" is really just the block we are placing
	public Ship placingShip;
	private int placingBlockType = 0;

	// Use this for initialization
	void Awake () {		
		main = this;

		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = GetComponent<Camera>().orthographicSize * 2;
		Bounds bounds = new Bounds(
			GetComponent<Camera>().transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));


		placingShip.SetBlock(0, 0, placingBlockType);
		placingShip.UpdateBlocks();
		foreach (var collider in placingShip.colliders) {
			Destroy(collider);
		}

		for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-25, 0), 60);
		}
	}

	void PlaceShipBlock(Vector2 pz, Block adjoiningBlock) {
		Ship ship;
		if (adjoiningBlock == null) {
			// new ship
			var shipObj = Instantiate(shipPrefab, new Vector3(pz.x, pz.y, 0f), Quaternion.identity) as GameObject;
			ship = shipObj.GetComponent<Ship>();
		} else {
			ship = adjoiningBlock.ship;
		}

		var bp = ship.WorldToBlockPos(pz);

		var block = ship.SetBlock(bp.x, bp.y, placingBlockType);
		placedBlocks.Add(block);
		ship.UpdateBlocks();

		/*if (adjoiningBlock != null) {
			var adjoiningPos = ship.blocks.Find(adjoiningBlock);
			
			if (blockPos.x < adjoiningPos.x) {
				block.transform.Rotate(Vector3.forward * -90);
				block.orientation = "left";
			} else if (blockPos.x > adjoiningPos.x) {
				block.transform.Rotate(Vector3.forward * 90);
				block.orientation = "right";
			} else if (blockPos.y > adjoiningPos.y) {
				block.transform.Rotate(Vector3.forward * 180);
				block.orientation = "up";
			} else {
				block.transform.Rotate(Vector3.forward * 0);
				block.orientation = "down";
			}
		}*/
	}


	void OnDrawGizmos() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			placingBlockType += 1;
			if (placingBlockType > Block.types.thruster) {
				placingBlockType = 0;
			}

			placingShip.blocks[0, 0].type = placingBlockType;
		}

		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		//adjoiningBlock = Block.FindNearestInRadius(pz, 0.2f);

		/*var nearbyBlocks = Block.FindInRadius(pz, 0.2f);
		if (adjoiningBlock != null) {
			adjoiningBlock.gameObject.GetComponent<Renderer>().material.color = Color.white;
		}
		adjoiningBlock = null;
		adjoiningShip = null;

		if (nearbyBlocks.Count() > 0) {
			adjoiningBlock = nearbyBlocks.First();
			adjoiningShip = adjoiningBlock.gameObject.transform.parent.GetComponent<Ship>();
			adjoiningBlock.gameObject.GetComponent<Renderer>().material.color = Color.green;
		}*/

		placingShip.transform.position = pz;

/*		if (adjoiningBlock != null) {
			var blockPos = adjoiningShip.WorldToBlockPos(pz);
			placingBlock.transform.position = adjoiningShip.BlockToWorldPos(blockPos);
			placingBlock.transform.rotation = adjoiningShip.transform.rotation;

			var adjoiningPos = adjoiningShip.blocks.Find(adjoiningBlock);
			
			if (blockPos.x < adjoiningPos.x) {
				placingBlock.transform.Rotate(Vector3.forward * -90);
			} else if (blockPos.x > adjoiningPos.x) {
				placingBlock.transform.Rotate(Vector3.forward * 90);
			} else if (blockPos.y > adjoiningPos.y) {
				placingBlock.transform.Rotate(Vector3.forward * 180);
			} else {
				placingBlock.transform.Rotate(Vector3.forward * 0);
			}

			placingBlock.orientation = "left";
		}*/

		if (Input.GetMouseButtonDown(0)) {			
			PlaceShipBlock(pz, adjoiningBlock);
			return;
		}

		// Block place undo
		if (Input.GetKeyDown(KeyCode.Z)) {
			var block = placedBlocks[placedBlocks.Count - 1];	
			var ship = block.ship;
			ship.blocks[block.pos] = null;
			ship.UpdateBlocks();
			placedBlocks.Remove(block);
		}

		// Scroll zoom
		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			Camera.main.orthographicSize--;
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			Camera.main.orthographicSize++;
		}

		if (activeShip == null)
			return;

		var rigid = activeShip.GetComponent<Rigidbody2D>();	

		if (Input.GetKey(KeyCode.W)) {
			activeShip.FireThrusters("down");		
		}

		if (Input.GetKey(KeyCode.S)) {
			activeShip.FireThrusters("up");
		}

		if (Input.GetKey(KeyCode.A)) {
			//rigid.AddTorque(0.1f);
			activeShip.FireThrusters("right");
		}

		if (Input.GetKey(KeyCode.D)) {
			//rigid.AddTorque(-0.1f);
			activeShip.FireThrusters("left");
		}

		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = 0.0f;
		}

		if (Input.GetMouseButton(1)) {
			var targetRotation = Quaternion.LookRotation((Vector3)pz - activeShip.transform.position);
			if (currentBeam == null) {
				currentBeam = Instantiate(tractorBeam, new Vector3(activeShip.transform.position.x, activeShip.transform.position.y, 1), targetRotation) as GameObject;
			}
			var ps = currentBeam.GetComponent<ParticleSystem>();
			if (targetRotation != currentBeam.transform.rotation) {
				ps.Clear();
			}
			currentBeam.transform.position = new Vector3(activeShip.transform.position.x, activeShip.transform.position.y, 1);
			currentBeam.transform.rotation = targetRotation;
			ps.startLifetime = Vector3.Distance(activeShip.transform.position, pz) / Math.Abs(ps.startSpeed);
			var dir = (pz - (Vector2)activeShip.transform.position);
			dir.Normalize();
			RaycastHit2D[] hits = Physics2D.CircleCastAll(activeShip.transform.position, 0.05f, dir, Vector3.Distance(activeShip.transform.position, pz));
			foreach (var hit in hits) {
				if (hit.collider.attachedRigidbody != null) {
					if (hit.collider.attachedRigidbody != rigid) {
						hit.collider.attachedRigidbody.AddForce(-dir);
					}
				}
			}
		} else {
			if (currentBeam != null) {
				Destroy(currentBeam);
			}
		}
	}
}
