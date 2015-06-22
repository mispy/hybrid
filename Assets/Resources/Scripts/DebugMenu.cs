using UnityEngine;
using System.Collections;

public class DebugMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			if (Ship.AtWorldPos(pz) == null)
				Generate.TestShip(pz);
		}
	}
}
