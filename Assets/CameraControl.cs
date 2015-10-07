using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraControl : MonoBehaviour {
	Camera camera { get { return Game.mainCamera; }}
	public Blockform lockedForm;
	public Blockform hoveredForm;
	public GameObject selector;

	public void Start() {
		lockedForm = Game.playerShip.form;
		selector = Pool.For("Selector").TakeObject();
	}

	public void ZoomIn() {	
		if (lockedForm != hoveredForm) {
			lockedForm = hoveredForm;
		}
		var newSize = (int)camera.orthographicSize >> 1;
		camera.orthographicSize = Mathf.Max(newSize, 4);
	}

	public void ZoomOut() {
		var newSize = (int)camera.orthographicSize << 1;
		camera.orthographicSize = Mathf.Min(newSize, 1 << 16);
	}
	
	// Update is called once per frame
	void Update () {
		var form = Blockform.ClosestTo(Game.mousePos).First();
		if (form != hoveredForm) {
			hoveredForm = form;
			selector.transform.SetParent(hoveredForm.transform);
			selector.transform.position = hoveredForm.transform.position;
			selector.transform.rotation = hoveredForm.transform.rotation;
			selector.transform.localScale = hoveredForm.bounds.size*1.2f;
			selector.SetActive(true);
		}

		if (form == Game.playerShip.form)
			selector.SetActive(false);


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

		if (lockedForm == Game.playerShip.form)
			camera.transform.rotation = lockedForm.transform.rotation;
		Game.MoveCamera(lockedForm.transform.position);
	}
}
