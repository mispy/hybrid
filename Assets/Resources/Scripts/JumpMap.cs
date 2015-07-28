using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var bounds = Util.GetCameraBounds();

		var beacons = new List<GameObject>();
		for (var i = 0; i < 20; i++) {
			var beacon = Pool.For("JumpBeacon").TakeObject();
			var x = Random.Range(bounds.min.x, bounds.max.x);
			var y = Random.Range(bounds.min.y, bounds.max.y);
			beacon.transform.position = new Vector2(x, y);
			var rend = beacon.GetComponent<Renderer>();
			rend.material.color = Util.RandomColor();	
			beacon.SetActive(true);	

			beacons.Add(beacon);
		}

		for (var i = 1; i < 20; i++) {
			var b1pos = beacons[i-1].transform.position;
			var b2pos = beacons[i].transform.position;

			var lineRenderer = beacons[i-1].AddComponent<LineRenderer>();
			var width = 0.05f;
			lineRenderer.SetWidth(width, width);
			lineRenderer.SetVertexCount(2);

			lineRenderer.SetPosition(0, b1pos);
			lineRenderer.SetPosition(1, b2pos);
			lineRenderer.SetColors(Color.white, Color.white);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
