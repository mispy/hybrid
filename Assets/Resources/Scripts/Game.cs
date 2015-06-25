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

	public Canvas canvas;
	public Text debugText;

	public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

	public static GameObject Prefab(string name) {
		if (!prefabs.ContainsKey(name)) {
			Debug.LogFormat("No prefab found for {0}. Available prefabs are: {1}", name, String.Join(", ", prefabs.Keys.ToArray()));
		}

		return prefabs[name];
	}

	public static IEnumerable<T> LoadPrefabs<T>(string path) {
		var resources = Resources.LoadAll(path);
		foreach (var obj in resources) {
			var gobj = obj as GameObject;
			if (gobj != null) {
				var comp = gobj.GetComponent<T>();
				if (comp != null) yield return comp;
			}
		}
	}

	public void BriefMessage(string message) {
		messageText.text = message;
		Invoke("ClearMessage", 2.0f);
	}

	public void ClearMessage() {
		messageText.text = "";
	}

	public static bool inputBlocked = false;
	public static string inputBlocker;

	public static void BlockInput(string blocker) {
		inputBlocked = true;
		inputBlocker = blocker;
	}

	public static void UnblockInput() {
		inputBlocked = false;
	}

	public ShipDesigner shipDesigner;
	public Text messageText;

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


		//Generate.EllipsoidShip(new Vector2(12, 0), 20, 10);

		//Generate.TestShip(new Vector2(5, 0));
		//for (var i = 0; i < 5; i++) {
		//	Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
		//}

		//InvokeRepeating("GenerateShip", 0.0f, 1.0f);

		debugMenu.GetComponent<DebugMenu>().LoadShip();
	}

	void GenerateShip() {
		Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
	}

	public Text debugMenu;

	// Update is called once per frame
	void Update() {
		if (Game.inputBlocked) return;

		if (Input.GetKeyDown(KeyCode.BackQuote)) {
			if (debugMenu.gameObject.activeInHierarchy)
				debugMenu.gameObject.SetActive(false);
			else
				debugMenu.gameObject.SetActive(true);
		}

		if (Input.GetKeyDown(KeyCode.F5)) {
			Save.SaveGame();
		}

		if (Input.GetKeyDown(KeyCode.F9)) {
			Save.LoadGame();
		}

		// Scroll zoom
		if (Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.orthographicSize > 6) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize >> 1;
			Debug.Log(Camera.main.orthographicSize);
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0 && Camera.main.orthographicSize < 96) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize << 1;
			Debug.Log(Camera.main.orthographicSize);
		}
	}
}
