using UnityEngine;
using System.Collections;

public class JumpMap : MonoBehaviour {

	// Use this for initialization
	void Start () {

		for (var i = 0; i < 20; i++) {
			var beacon = Pool.For("JumpBeacon").TakeObject();
			var x = Random.Range(-100, 100);
			var y = Random.Range(-100, 100);
			beacon.transform.position = new Vector2(x, y);
			beacon.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
