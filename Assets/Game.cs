using UnityEngine;
using UnityEngine.UI;
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
	public Text debugText;

	public Ship testShip;

	private List<Block> placedBlocks = new List<Block>();

	public Crew player;

	public Ship activeShip = null;

	private Block adjoiningBlock = null;

	public Blueprint placingShip;
	private int placingBlockType = 0;

	public Texture2D[] blockSprites;
		
	// Use this for initialization
	void Awake () {		
		if (Game.main != null) return;
		Game.main = this;

		Shields.prefab = Resources.Load("Shields") as GameObject;
		Blueprint.prefab = Resources.Load("Blueprint") as GameObject;
		Ship.prefab = Resources.Load("Ship") as GameObject;

		Block.Setup(blockSprites);
		Pool.CreatePools();		
						
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = GetComponent<Camera>().orthographicSize * 2;
		Bounds bounds = new Bounds(
			GetComponent<Camera>().transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));

		Save.LoadGame();

		/*for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-60, 0), 30);
		}

		Generate.TestShip(new Vector2(5, 0));

		testShip =	Generate.EllipsoidShip(new Vector2(18, 0), 20, 10);*/
	}

	// Update is called once per frame
	void Update() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		if (Input.GetKeyDown(KeyCode.F5)) {
			Save.SaveGame();
		}

		if (Input.GetKeyDown(KeyCode.F9)) {
			Save.LoadGame();
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
			activeShip.FireThrusters(Orientation.up);		
		}

		if (Input.GetKey(KeyCode.S)) {
			activeShip.FireThrusters(Orientation.down);
		}

		if (Input.GetKey(KeyCode.A)) {
			//rigid.AddTorque(0.1f);
			activeShip.FireThrusters(Orientation.right);
		}

		if (Input.GetKey(KeyCode.D)) {
			//rigid.AddTorque(-0.1f);
			activeShip.FireThrusters(Orientation.left);
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
						hit.collider.attachedRigidbody.AddForce(-dir * Block.defaultMass * 10);
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
