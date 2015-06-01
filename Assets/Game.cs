using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
	public static Game main;

	public GameObject shipPrefab;

	public GameObject tractorBeam;
	public GameObject currentBeam;
	public GameObject thrustPrefab;
	public GameObject wallColliderPrefab;
	public GameObject floorColliderPrefab;
	public GameObject particleBeamPrefab;

	private List<Block> placedBlocks = new List<Block>();

	public Crew player;

	public Ship activeShip = null;

	private Block adjoiningBlock = null;

	// this "ship" is really just the block we are placing
	public Ship placingShip;
	private int placingBlockType = 0;

	public Texture2D[] blockSprites;
		
	// Use this for initialization
	void Awake () {		
		if (Game.main != null) return;
		Game.main = this;

		Block.Setup(blockSprites);
		Pool.CreatePools();		
				
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = GetComponent<Camera>().orthographicSize * 2;
		Bounds bounds = new Bounds(
			GetComponent<Camera>().transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));

		var placingShipObj = Pool.Ship.TakeObject();
		placingShip = placingShipObj.GetComponent<Ship>();
		placingShip.hasCollision = false;
		placingShip.blocks[0, 0] = new Block(placingBlockType);
		placingShipObj.SetActive(true);

		for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-60, 0), 60);
		}

		Generate.TestShip(new Vector2(5, 0));
	}

	void PlaceShipBlock(Vector2 pz, Block adjoiningBlock) {
		Ship ship;
		if (adjoiningBlock == null) {
			// new ship
			var shipObj = Pool.Ship.TakeObject();
			ship = shipObj.GetComponent<Ship>();
			shipObj.SetActive(true);
		} else {
			ship = adjoiningBlock.ship;
		}

		var bp = ship.WorldToBlockPos(pz);

		var block = new Block(placingShip.blocks[0,0].type);
		block.orientation = placingShip.blocks[0,0].orientation;
		ship.blocks[bp] = block;
		placedBlocks.Add(block);
		ship.UpdateBlocks();
	}


	void OnDrawGizmos() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			placingBlockType += 1;
			if (placingBlockType >= blockSprites.Length) {
				placingBlockType = 0;
			}

			placingShip.blocks[0, 0] = new Block(placingBlockType);
			placingShip.UpdateBlocks();
		}

		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 


		var nearbyBlocks = Block.FindInRadius(pz, Block.worldSize);
		//if (adjoiningBlock != null) {
		//	adjoiningBlock.gameObject.GetComponent<Renderer>().material.color = Color.white;
		//}
		adjoiningBlock = null;
		if (nearbyBlocks.Count() > 0) {
			foreach (var block in nearbyBlocks) {
				// don't adjoin to itself
				if (block != block.ship.BlockAtWorldPos(pz)) {
					adjoiningBlock = block;
				    break;
				}
			}
			//adjoiningBlock.gameObject.GetComponent<Renderer>().material.color = Color.green;		}
		}

		placingShip.transform.position = pz;
		if (adjoiningBlock != null) {
			var ship = adjoiningBlock.ship;
			var blockPos = ship.WorldToBlockPos(pz);
			placingShip.transform.position = ship.BlockToWorldPos(blockPos);
			placingShip.transform.rotation = ship.transform.rotation;

			Vector2 ori;
			if (blockPos.x < adjoiningBlock.pos.x) {
				ori = -Vector2.right;
			} else if (blockPos.x > adjoiningBlock.pos.x) {
				ori = Vector2.right;
			} else if (blockPos.y > adjoiningBlock.pos.y) {
				ori = -Vector2.up;
			} else {
				ori = Vector2.up;
			}


			if (ori != placingShip.blocks[0, 0].orientation) {
				var block = placingShip.blocks[0, 0];
				block.orientation = ori;
				placingShip.blocks[0, 0] = block;
				placingShip.UpdateBlocks();
			}
		}

		if (Input.GetMouseButton(0)) {			
			PlaceShipBlock(pz, adjoiningBlock);
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
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize >> 1;
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize << 1;
		}

		if (activeShip == null)
			return;

		var rigid = activeShip.rigidBody;	

		if (Input.GetKey(KeyCode.W)) {
			activeShip.FireThrusters(Vector2.up);		
		}

		if (Input.GetKey(KeyCode.S)) {
			activeShip.FireThrusters(-Vector2.up);
		}

		if (Input.GetKey(KeyCode.A)) {
			//rigid.AddTorque(0.1f);
			activeShip.FireThrusters(Vector2.right);
		}

		if (Input.GetKey(KeyCode.D)) {
			//rigid.AddTorque(-0.1f);
			activeShip.FireThrusters(-Vector2.right);
		}

		if (Input.GetKey(KeyCode.Space)) {
			activeShip.FireLasers();
		}

		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
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
			RaycastHit[] hits = Physics.SphereCastAll(activeShip.transform.position, 0.05f, dir, Vector3.Distance(activeShip.transform.position, pz));
			foreach (var hit in hits) {
				if (hit.collider.attachedRigidbody != null) {
					if (hit.collider.attachedRigidbody != rigid) {
						hit.collider.attachedRigidbody.AddForce(-dir * Block.mass * 10);
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
