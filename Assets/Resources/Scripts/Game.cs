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

	// cached main camera
	// Camera.main seems to perform some kind of expensive lookup
	public static Camera mainCamera;

	// cached mouse position in world coordinates
	public static Vector3 mousePos;
	public static Ship playerShip;
	public static ActiveSector activeSector;

	public Canvas canvas;
	public Text debugText;
	public JumpMap jumpMap;

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

	public static IEnumerable<Texture2D> LoadTextures(string path) {
		var resources = Resources.LoadAll(path);
		foreach (var obj in resources) {
			var tex = obj as Texture2D;
			if (tex != null) {
				yield return tex;
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
	public WeaponSelect weaponSelect;
	public Text messageText;

	void MakeUniverse() {
		var cosmicWidth = 100;
		var cosmicHeight = 100;

		for (var i = 0; i < 100; i++) {
			var sector = new Sector();
			sector.cosmicX = Random.Range(-cosmicWidth/2, cosmicWidth/2);
			sector.cosmicY = Random.Range(-cosmicHeight/2, cosmicHeight/2);
			SectorManager.Add(sector);	
		}
		
		for (var i = 0; i < 20; i++) {
			var ship = ShipManager.Unpack(ShipManager.RandomTemplate());
			var sector = Util.GetRandom(SectorManager.all);
			ship.sector = sector;
			ShipManager.Add(ship);
		}
	}

	// Use this for initialization
	void Awake () {		
		Game.activeSector = GetComponentInChildren<ActiveSector>();
		Game.main = this;

		var resources = Resources.LoadAll("Prefabs");
		
		foreach (var obj in resources) {
			var gobj = obj as GameObject;
			if (gobj != null) {
				prefabs[obj.name] = gobj;
			}
		}

		Tile.Setup();
		Block.Setup();
		Pool.CreatePools();		
		ShipManager.LoadTemplates();

		MakeUniverse();

		Game.playerShip = Util.GetRandom(ShipManager.all);
		Game.activeSector.LoadSector(Game.playerShip.sector);
		//Save.LoadGame();


		//Generate.EllipsoidShip(new Vector2(12, 0), 20, 10);

		//Generate.TestShip(new Vector2(5, 0));
		//for (var i = 0; i < 5; i++) {
		//	Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
		//}
		//InvokeRepeating("GenerateShip", 0.0f, 1.0f);

		for (var i = 0; i < 100; i++) {
			//debug.MakeAsteroid(new Vector2(Random.Range(-sectorSize, sectorSize), Random.Range(-sectorSize, sectorSize)));
		}
	}

	void GenerateShip() {
		Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
	}

	void Start() {
		Game.mainCamera = Camera.main;
	}

	public Text debugMenu;

	// Update is called once per frame
	void Update() {
		mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		mousePos.z = 0f;

		if (Game.inputBlocked) return;

		if (Input.GetKeyDown(KeyCode.BackQuote)) {
			if (debugMenu.gameObject.activeInHierarchy)
				debugMenu.gameObject.SetActive(false);
			else
				debugMenu.gameObject.SetActive(true);
		}

		if (Input.GetKeyDown(KeyCode.F5)) {
			//Save.SaveGame();
		}

		if (Input.GetKeyDown(KeyCode.F9)) {
			//Save.LoadGame();
		}

		// Scroll zoom
		if (Input.GetKeyDown(KeyCode.Equals) && Camera.main.orthographicSize > 4) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize >> 1;
			//Debug.Log(Camera.main.orthographicSize);
		} else if (Input.GetKeyDown(KeyCode.Minus) && Camera.main.orthographicSize < 64) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize << 1;
			//Debug.Log(Camera.main.orthographicSize);
		}

		// Scroll zoom
		if (Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.orthographicSize > 4) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize >> 1;
			//Debug.Log(Camera.main.orthographicSize);
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {// && Camera.main.orthographicSize < 64) {
			Camera.main.orthographicSize = (int)Camera.main.orthographicSize << 1;
			//Debug.Log(Camera.main.orthographicSize);
		}
	}
}
