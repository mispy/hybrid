using UnityEngine;
using System.Collections;

public class DebugControl : MonoBehaviour {
	Blockform selectedShip;

	// Use this for initialization
	void Start () {
		selectedShip = Game.activeSector.blockforms[1];
	}
	
	// Update is called once per frame
	void Update () {		
		if (Input.GetMouseButtonDown(0)) {
			selectedShip = Blockform.AtWorldPos(Game.mousePos);
		}

		if (selectedShip != null) {
			Game.MoveCamera(selectedShip.transform.position);
			Game.mainCamera.transform.rotation = selectedShip.transform.rotation;
		}

		// Scroll zoom
		if (Input.GetKeyDown(KeyCode.Equals) && Game.mainCamera.orthographicSize > 4) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize >> 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		} else if (Input.GetKeyDown(KeyCode.Minus) && Game.mainCamera.orthographicSize < 64) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize << 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		}
		
		// Scroll zoom
		if (Input.GetAxis("Mouse ScrollWheel") > 0 && Game.mainCamera.orthographicSize > 4) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize >> 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {// && Game.mainCamera.orthographicSize < 64) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize << 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		}
	}
}
