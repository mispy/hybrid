using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JumpMap : MonoBehaviour {
		
	float jumpRange = 5f;
	LineRenderer lineRenderer;
	List<GameObject> beacons = new List<GameObject>();

	// Use this for initialization
	void Start () {
		var bounds = Util.GetCameraBounds();
		gameObject.AddComponent<LineRenderer>();

		for (var i = 0; i < 20; i++) {
			var beacon = Pool.For("JumpBeacon").TakeObject();
			beacon.transform.parent = transform;
			var x = Random.Range(bounds.min.x, bounds.max.x);
			var y = Random.Range(bounds.min.y, bounds.max.y);
			beacon.transform.position = new Vector2(x, y);
			var rend = beacon.GetComponent<Renderer>();
			rend.material.color = Util.RandomColor();	
			beacon.SetActive(true);	

			beacons.Add(beacon);
		}

		var positions = new List<Vector3>();
	}

	void DrawConnections(GameObject beacon) {
		var lineRenderer = gameObject.GetComponent<LineRenderer>();

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
		lineRenderer.SetColors(Color.white, Color.white);
		lineRenderer.SetVertexCount(points.Count);

		for (var i = 0; i < points.Count; i++) {
			lineRenderer.SetPosition(i, points[i]);
		}

	}


	void AddConnection(GameObject b1, GameObject b2) {

		lineRenderer.SetPosition(0, b1.transform.position);
		lineRenderer.SetPosition(1, b2.transform.position);
		lineRenderer.SetColors(Color.white, Color.white);
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		var beacon = beacons.OrderBy ((b) => Vector3.Distance (b.transform.position, pz)).First ();
		DrawConnections(beacon);
	}
}
