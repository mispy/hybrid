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

	public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	public static GameObject Prefab(string name) {
		if (!prefabs.ContainsKey(name)) {
			Debug.LogFormat("No prefab found for {0}. Available prefabs are: {1}", name, String.Join(", ", prefabs.Keys.ToArray()));
		}

		return prefabs[name];
	}

	// Use this for initialization
	void Awake () {		
		Game.main = this;

		var resources = Resources.LoadAll("Prefabs");
		
		foreach (var obj in resources) {
			var gobj = obj as GameObject;
			if (gobj != null) {
				prefabs[obj.name] = gobj;
			}
		}
		
		Block.Setup();
		Pool.CreatePools();		

		//Save.LoadGame();

		/*for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-60, 0), 30);
		}*/

		//Generate.EllipsoidShip(new Vector2(12, 0), 20, 10);
		for (var i = 0; i < 30; i++) {
			Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
		}

		InvokeRepeating("GenerateShip", 0.0f, 1.0f);
	}

	void GenerateShip() {
		Generate.TestShip(new Vector2(Random.Range(-25, 25), Random.Range(-25, 25)));
	}

	// Update is called once per frame
	void Update() {
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
	}
}
