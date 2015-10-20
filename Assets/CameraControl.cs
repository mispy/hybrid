using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraControl : MonoBehaviour {
	Camera camera { get { return Game.mainCamera; }}
	public Blockform lockedForm;
	public Blockform hoveredForm;
	public GameObject selector;
    Vector2 cameraOffset = new Vector2(0, 0);

	public void Start() {
		lockedForm = Game.playerShip.form;
		selector = Pool.For("Selector").TakeObject();
	}

	public void ZoomIn() {	
		if (lockedForm != hoveredForm) {
			lockedForm = hoveredForm;
		}

        var shipViewPos = camera.WorldToViewportPoint(lockedForm.transform.position);

		var newSize = (int)camera.orthographicSize >> 1;
		camera.orthographicSize = Mathf.Max(newSize, 4);

        var newShipPos = camera.ViewportToWorldPoint(shipViewPos);
        cameraOffset = lockedForm.transform.InverseTransformVector(newShipPos - lockedForm.transform.position);
	}

	public void ZoomOut() {
        var shipViewPos = camera.WorldToViewportPoint(lockedForm.transform.position);

		var newSize = (int)camera.orthographicSize << 1;
		camera.orthographicSize = Mathf.Min(newSize, 1 << 16);

        var newShipPos = camera.ViewportToWorldPoint(shipViewPos);
        cameraOffset = lockedForm.transform.InverseTransformVector(newShipPos - lockedForm.transform.position);
    }
	
	// Update is called once per frame
	void Update () {
		var form = Blockform.ClosestTo(Game.mousePos).First();
		if (form != hoveredForm) {
			hoveredForm = form;
		}

		if (hoveredForm != Game.playerShip.form) {
			selector.SetActive(true);
			selector.transform.position = hoveredForm.transform.position;
			selector.transform.rotation = hoveredForm.transform.rotation;
			selector.transform.localScale = hoveredForm.box.bounds.size*1.2f;
		} else {
			selector.SetActive(false);
		}


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

		if (lockedForm == null)
			lockedForm = Game.playerShip.form;
		if (lockedForm == Game.playerShip.form)
			camera.transform.rotation = lockedForm.transform.rotation;

		Game.MoveCamera(lockedForm.transform.position + lockedForm.transform.TransformVector(cameraOffset));
	}
}
