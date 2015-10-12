using UnityEngine;
using System.Collections;

public class AnchoredPanning : MonoBehaviour {
	Vector2 startMouseOffset;
	Vector2 startCameraOffset;
	float maxCameraDist = 100f;
	float heldFor = 0f;
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
		//Game.mainCamera.transform.rotation = form.transform.rotation;

		if (Input.GetMouseButton(2)) {
			if (!isPanning) {
				isPanning = true;
				heldFor = 0f;
				startMouseOffset = form.transform.InverseTransformPoint(Game.mousePos);
				startCameraOffset = Game.mainCamera.transform.localPosition;
			}

			heldFor += Time.deltaTime;

			var newMouseOffset = (Vector2)form.transform.InverseTransformPoint(Game.mousePos);
			var newCameraOffset = startCameraOffset + (newMouseOffset - startMouseOffset);

			if (newCameraOffset.magnitude > maxCameraDist) {
				newCameraOffset = newCameraOffset.normalized * maxCameraDist;
			}

			Game.MoveCamera((Vector2)form.transform.position + newCameraOffset);
		} else {
			if (isPanning) {
				isPanning = false;
				if (heldFor < 0.2f && Vector2.Distance(Game.mousePos, (Vector2)form.transform.position + startCameraOffset) < 2f) {
					Game.MoveCamera(form.transform.position);
				}
			}
		}
	}
}
