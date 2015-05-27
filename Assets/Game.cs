using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
	public static Game main;

	public float tileWidth;
	public float tileHeight;
	public GameObject tilePrefab;
	public Sprite[] tileTypes;
	public GameObject shipPrefab;

	private int shipIndex = 10;
	public List<GameObject> ships;

	public GameObject tractorBeam;
	public GameObject currentBeam;

	private int tileTypeIndex = 0;
	

	// Use this for initialization
	void Awake () {		
		var renderer = tilePrefab.GetComponent<Renderer>();
		tileWidth = renderer.bounds.size.x;
		tileHeight = renderer.bounds.size.y;
		main = this;

		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = GetComponent<Camera>().orthographicSize * 2;
		Bounds bounds = new Bounds(
			GetComponent<Camera>().transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));


		for (var i = 0; i < 10; i++) {
			var x = Random.Range(bounds.min.x, bounds.max.x);
			var y = Random.Range(bounds.min.y, bounds.max.y);
			PlaceShipBlock(new Vector2(x, y), i);
		}
	}

	void PlaceShipBlock(Vector2 pz, int index) {
		if (index >= ships.Count) {
			// new ship
			ships.Add(Instantiate(shipPrefab, new Vector3(pz.x, pz.y, 0f), Quaternion.identity) as GameObject);
		}

		var ship = ships[index];
		
		var block = Instantiate(tilePrefab, new Vector3(pz.x, pz.y, 0f), Quaternion.identity) as GameObject;
		var rigid = block.GetComponent<Rigidbody2D>();
		block.transform.rotation = ship.transform.rotation;
		block.transform.parent = ship.transform;
		block.GetComponent<SpriteRenderer>().sprite = tileTypes[tileTypeIndex];
		
		var tileX = (int)Math.Round(block.transform.localPosition.x / tileWidth);
		var tileY = (int)Math.Round(block.transform.localPosition.y / tileHeight);
		var localPos = new Vector2((float)tileX*tileWidth, (float)tileY*tileHeight);

		var shipScript = ship.GetComponent<Ship>();
		if (shipScript.blocks[tileX, tileY] != null) {
			Destroy(shipScript.blocks[tileX, tileY]);
			shipScript.blocks[tileX, tileY] = null;
		}

		shipScript.blocks[tileX, tileY] = block;
		block.transform.localPosition = localPos;

		shipScript.RecalculateMass();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Keypad1)) {
			shipIndex = 11;
		} else if (Input.GetKeyDown (KeyCode.Keypad1)) {
			shipIndex = 12;
		}

		if (Input.GetKeyDown(KeyCode.Tab)) {
			tileTypeIndex += 1;
			if (tileTypeIndex >= tileTypes.Length) {
				tileTypeIndex = 0;
			}
		}

		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (Input.GetMouseButton(0)) {			
			PlaceShipBlock(pz, shipIndex);
			return;
		}

		if (shipIndex >= ships.Count) {
			return;
		}

		var ship = ships[shipIndex];
		var rigid = ship.GetComponent<Rigidbody2D>();		

		if (Input.GetKey(KeyCode.W)) {
			rigid.AddRelativeForce(new Vector2(0, 3));
		}

		if (Input.GetKey(KeyCode.A)) {
			rigid.AddTorque(0.1f);
		}

		if (Input.GetKey(KeyCode.D)) {
			rigid.AddTorque(-0.1f);
		}

		if (Input.GetKey(KeyCode.S)) {
			rigid.AddRelativeForce(new Vector2(0, -3));
		}

		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = 0.0f;
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			Camera.main.orthographicSize--;
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			Camera.main.orthographicSize++;
		}

		if (Input.GetMouseButton(1)) {
			var targetRotation = Quaternion.LookRotation((Vector3)pz - ship.transform.position);
			if (currentBeam == null) {
				currentBeam = Instantiate(tractorBeam, new Vector3(ship.transform.position.x, ship.transform.position.y, 1), targetRotation) as GameObject;
			}
			var ps = currentBeam.GetComponent<ParticleSystem>();
			if (targetRotation != currentBeam.transform.rotation) {
				ps.Clear();
			}
			currentBeam.transform.position = new Vector3(ship.transform.position.x, ship.transform.position.y, 1);
			currentBeam.transform.rotation = targetRotation;
			ps.startLifetime = Vector3.Distance(ship.transform.position, pz) / Math.Abs(ps.startSpeed);
			var dir = (pz - (Vector2)ship.transform.position);
			dir.Normalize();
			RaycastHit2D[] hits = Physics2D.CircleCastAll(ship.transform.position, 0.05f, dir, Vector3.Distance(ship.transform.position, pz));
			foreach (var hit in hits) {
				if (hit.collider.attachedRigidbody != rigid) {
					hit.collider.attachedRigidbody.AddForce(-dir);
				}
			}
		} else {
			if (currentBeam != null) {
				Destroy(currentBeam);
			}
		}
	}
}
