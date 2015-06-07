using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour, ISerializationCallbackReceiver {
	public static Game main;

	public Text debugText;

	private List<Block> placedBlocks = new List<Block>();

	public Ship activeShip = null;

	private Block adjoiningBlock = null;

	public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
	public static GameObject Prefab(string name) {
		return prefabs[name];
	}

	public List<GameObject> prefabList = new List<GameObject>();

	public void OnBeforeSerialize() {
	}

	public void OnAfterDeserialize() {
		foreach (var prefab in prefabList) {
			prefabs[prefab.name] = prefab;
		}
	}

	// Use this for initialization
	void Awake () {		
		var resources = Resources.LoadAll("");
		foreach (var obj in resources) {
			var gobj = obj as GameObject;
			if (gobj != null) {
				prefabList.Add(gobj);
				prefabs[obj.name] = gobj;
			}
		}

		Block.manager = GetComponent<BlockManager>();
		Block.manager.Setup();

		Pool.manager = GetComponent<PoolManager>();
		Pool.CreatePools();		
		
		/*float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = Camera.main.orthographicSize * 2;
		Bounds bounds = new Bounds(
			Camera.main.transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));*/

		//Save.LoadGame();


		for (var i = 0; i < 1; i++) {
			Generate.Asteroid(new Vector2(-60, 0), 30);
		}

		Generate.TestShip(new Vector2(5, 0));
	}

	void OnEnable() {

		Game.main = this;
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
