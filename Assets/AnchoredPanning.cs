using UnityEngine;
using System.Collections;

public class AnchoredPanning : MonoBehaviour {
	Vector2 startMouseOffset;
	Vector2 startCameraOffset;
	float maxCameraDist = 100f;
	bool isPanning = false;

	// Use this for initialization
	void Start () {
		var form = Game.playerShip.form;

		Game.mainCamera.transform.SetParent(form.transform);
		Game.MoveCamera(form.transform.position);
	}
	
	// Update is called once per frame
	void Update () {		
		var form = Game.playerShip.form;
		Game.mainCamera.transform.rotation = form.transform.rotation;

		if (Input.GetMouseButton(2)) {
			if (!isPanning) {
				isPanning = true;
				startMouseOffset = Game.mousePos - (Vector2)form.transform.position;
				startCameraOffset = Game.mainCamera.transform.localPosition;
			}

			var vec = (Game.mousePos - (Vector2)form.transform.position) - startMouseOffset;
			vec /= 2.0f;
			var newCameraOffset = startCameraOffset + vec;

			if (newCameraOffset.magnitude > maxCameraDist) {
				newCameraOffset = newCameraOffset.normalized * maxCameraDist;
			}

			Game.MoveCamera((Vector2)form.transform.position + newCameraOffset);
		} else {
			isPanning = false;
		}
	}
}
