using UnityEngine;
using System.Collections;

public class JumpMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var bounds = Util.GetCameraBounds();

		var beacons = new List<Beacon>();
		for (var i = 0; i < 20; i++) {
			var beacon = Pool.For("JumpBeacon").TakeObject();
			var x = Random.Range(bounds.min.x, bounds.max.x);
			var y = Random.Range(bounds.min.y, bounds.max.y);
			beacon.transform.position = new Vector2(x, y);
			var rend = beacon.GetComponent<Renderer>();
			rend.material.color = Util.RandomColor();	
			beacon.SetActive(true);	
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
