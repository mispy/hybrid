using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraControl : MonoBehaviour {
	new Camera camera { get { return Game.mainCamera; }}
    Vector2 cameraOffset = new Vector2(0, 0);

    public Transform locked;

	public void Start() {
	}

	public void ZoomIn() {	
        var shipViewPos = camera.WorldToViewportPoint(locked.transform.position);

		var newSize = (int)camera.orthographicSize >> 1;
		camera.orthographicSize = Mathf.Max(newSize, 4);

        var newShipPos = camera.ViewportToWorldPoint(shipViewPos);
        cameraOffset = locked.transform.InverseTransformVector(newShipPos - locked.transform.position);
	}

	public void ZoomOut() {
        var shipViewPos = camera.WorldToViewportPoint(locked.transform.position);

		var newSize = (int)camera.orthographicSize << 1;
		camera.orthographicSize = Mathf.Min(newSize, 1 << 16);

        var newShipPos = camera.ViewportToWorldPoint(shipViewPos);
        cameraOffset = locked.transform.InverseTransformVector(newShipPos - locked.transform.position);
    }
	
	// Update is called once per frame
	void Update () {
        if (Game.localPlayer == null) return;
        locked = Game.localPlayer.transform;

		if (Input.GetKeyDown(KeyCode.Equals)) {
			ZoomIn();
		} else if (Input.GetKeyDown(KeyCode.Minus)) {
			ZoomOut();
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			ZoomIn();
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			ZoomOut();
		}

		camera.transform.rotation = locked.transform.rotation;
		Game.MoveCamera(locked.transform.position + locked.transform.TransformVector(cameraOffset));
	}
}
