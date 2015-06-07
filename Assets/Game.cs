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

	public Text debugText;


	public Ship activeShip = null;

	public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	public static GameObject Prefab(string name) {
		return prefabs[name];
	}

	// Use this for initialization
	void Awake () {		
		Game.main = this;

		var resources = Resources.LoadAll("");
		foreach (var obj in resources) {
			var gobj = obj as GameObject;
			if (gobj != null) {
				prefabs[obj.name] = gobj;
			}
		}


		Block.Setup();
		Pool.CreatePools();		

		//Save.LoadGame();

		for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-60, 0), 30);
		}

		Generate.TestShip(new Vector2(5, 0));
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
			activeShip.StartTractorBeam(pz);
		} else {
			activeShip.StopTractorBeam();
		}
	}
}
