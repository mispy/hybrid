using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpMap : MonoBehaviour {		
	float jumpRange = 5f;
	Color hoverColor = new Color (0.95f, 0.64f, 0.38f, 0.05f);
	Color currentColor = new Color (0f, 1f, 1f, 1f);

	LineRenderer lineRenderer;
	List<JumpBeacon> beacons = new List<JumpBeacon>();
	List<JumpShip> ships = new List<JumpShip>();
	JumpBeacon selectedBeacon;
	Canvas canvas;
	GameObject selector;
	JumpShip playerShip;

	Button enterButton;
	Button foldButton;
	Button waitButton;
	Button stopWaitButton;
	Camera camera;

	bool isWaiting = false;
	bool isJumping = false;

	public static void Activate() {
		Game.activeSector.gameObject.SetActive(false);
		Game.main.jumpMap.gameObject.SetActive(true);
	}

	void OnEnable() {
		playerShip.ship = Game.playerShip;
		playerShip.tiles.SetBlocks(Game.playerShip.blocks);
		playerShip.Rescale();
	}

	void OnDisable() {
		Game.activeSector.gameObject.SetActive(true);
	}

	// Use this for initialization
	void Awake() {				
		camera = GetComponentInChildren<Camera>();
		camera.orthographicSize = 4;
		var bounds = Util.GetCameraBounds(camera);

		for (var i = 0; i < 20; i++) {
			var beacon = Pool.For("JumpBeacon").Take<JumpBeacon>();
			beacon.transform.parent = transform;
			var x = Random.Range(bounds.min.x, bounds.max.x);
			var y = Random.Range(bounds.min.y, bounds.max.y);
			beacon.transform.position = new Vector2(x, y);
			var rend = beacon.GetComponent<Renderer>();
			//rend.material.color = Util.RandomColor();	
			beacon.gameObject.SetActive(true);	
			
			beacons.Add(beacon);
		}

		for (var i = 0; i < 20; i++) {
			var ship = ShipManager.Create(ShipManager.RandomTemplate());
			var jumpShip = JumpShip.For(ship);
			jumpShip.transform.parent = transform;
			jumpShip.gameObject.SetActive(true);
			ships.Add(jumpShip);

			var beacon = Util.GetRandom(beacons);
			beacon.PlaceShip(jumpShip);
		}

		playerShip = Pool.For("JumpShip").Take<JumpShip>();
		playerShip.transform.parent = transform;
		playerShip.gameObject.SetActive(true);
		ships.Add(playerShip);

		canvas = GetComponentInChildren<Canvas>();
		selector = Pool.For("Selector").TakeObject();
		selector.transform.parent = transform;
		selector.SetActive(true);


		var positions = new List<Vector3>();

		DrawFactions();

		foreach (var button in GetComponentsInChildren<Button>(includeInactive: true)) {
			if (button.name == "FoldButton") {
				foldButton = button;
				foldButton.onClick.AddListener(() => FoldJump(selectedBeacon));
			}

			if (button.name == "EnterButton") {
				enterButton = button;
				enterButton.onClick.AddListener(() => EnterSector());
			}

			if (button.name == "WaitButton") {
				waitButton = button;
				waitButton.onClick.AddListener(() => Wait());
			}

			if (button.name == "StopWaitButton") {
				stopWaitButton = button;
				stopWaitButton.onClick.AddListener(() => StopWait());
			}
		}

	}

	void EnterSector() {
		Game.activeSector.LoadSector(playerShip.currentBeacon);
		Game.activeSector.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	void Wait() {
		isWaiting = true;
		waitButton.gameObject.SetActive(false);
		stopWaitButton.gameObject.SetActive(true);
	}

	void StopWait() {
		isWaiting = false;
		waitButton.gameObject.SetActive(true);
		stopWaitButton.gameObject.SetActive(false);
	}

	void FoldJump(JumpBeacon beacon) {
		Game.activeSector.UnloadSector();
		playerShip.FoldJump(beacon);
	}

	void SelectBeacon(JumpBeacon beacon) {
		selectedBeacon = beacon;
		selector.transform.parent = beacon.transform.parent;
		selector.transform.position = beacon.transform.position;
	}

	void DrawFactions() {
		var beacon = beacons[0];
		var circle = Pool.For("FactionCircle").TakeObject();
		circle.transform.parent = beacon.transform;
		circle.transform.localPosition = Vector3.zero;
		circle.GetComponent<SpriteRenderer>().color = Color.green;
		circle.SetActive(true);
	}

	// Only happens when in motion or waiting
	void JumpUpdate() {
		foreach (var ship in ships) {
			if (ship != playerShip && ship.destBeacon == null && Random.Range(0, 100) == 99) {
				var beacon = Util.GetRandom(beacons);
				ship.FoldJump(beacon);
			}
			ship.JumpUpdate();
		}
	}

	// Update is called once per frame
	void Update() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		var nearMouseBeacon = beacons.OrderBy ((b) => Vector3.Distance (b.transform.position, pz)).First();
		
		if (Input.GetKeyDown(KeyCode.J)) {
			gameObject.SetActive(false);
		}

		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButtonDown(0)) {
				SelectBeacon(nearMouseBeacon);
			}
		}
		
		if (selectedBeacon == playerShip.currentBeacon) {
			foldButton.gameObject.SetActive(false);
			enterButton.gameObject.SetActive(true);
		} else {
			foldButton.gameObject.SetActive(true);
			enterButton.gameObject.SetActive(false);
		}

		if (isWaiting || playerShip.destBeacon != null) JumpUpdate();
	}
}
