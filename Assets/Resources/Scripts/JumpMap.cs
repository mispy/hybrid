using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JumpMap : MonoBehaviour {
		
	float jumpRange = 5f;
	Color hoverColor = new Color (0.95f, 0.64f, 0.38f, 0.05f);
	Color currentColor = new Color (0f, 1f, 1f, 1f);

	LineRenderer lineRenderer;
	List<JumpBeacon> beacons = new List<JumpBeacon>();
	JumpBeacon hoverBeacon;
	JumpBeacon selectedBeacon;
	Canvas canvas;
	GameObject selector;

	void OnEnable() {
		Game.main.currentSector.gameObject.SetActive(false);
	}

	void OnDisable() {
		Game.main.currentSector.gameObject.SetActive(true);
	}

	// Use this for initialization
	void Start() {		
		canvas = GetComponentInChildren<Canvas>();
		selector = Pool.For("Selector").TakeObject();
		selector.SetActive(true);

		var bounds = Util.GetCameraBounds();

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

		var positions = new List<Vector3>();

		DrawFactions();
	}

	void DrawConnections(JumpBeacon beacon, Color color) {
		var lineRenderer = beacon.gameObject.GetComponent<LineRenderer>();

		var points = new List<Vector3>();
		foreach (var b2 in beacons) {
			if (Vector3.Distance(beacon.transform.position, b2.transform.position) < jumpRange) {
				points.Add(beacon.transform.position);
				points.Add(b2.transform.position);
				points.Add(beacon.transform.position);
			}
		}

		var width = 0.05f;
		lineRenderer.SetWidth(width, width);
		lineRenderer.SetVertexCount(points.Count);

		for (var i = 0; i < points.Count; i++) {
			lineRenderer.SetPosition(i, points[i]);
		}

		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.material.color = color;
		lineRenderer.SetColors(color, color);
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

	// Update is called once per frame
	void Update() {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		var nearMouseBeacon = beacons.OrderBy ((b) => Vector3.Distance (b.transform.position, pz)).First ();

		if (Input.GetMouseButtonDown(0)) {
			SelectBeacon(nearMouseBeacon);
		}

		if (Input.GetKeyDown(KeyCode.J)) {
			gameObject.SetActive(false);
		}
	}
}
