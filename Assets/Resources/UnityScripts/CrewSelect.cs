using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CrewSelect : MonoBehaviour {
	void Start () {
		foreach (Transform child in transform) {
			Destroy(child.gameObject);
		}

		int i = 0;
		float buttonHeight;

		foreach (var crew in Game.playerShip.crew) {
			var button = Pool.For("CrewButton").Take<Button>();
			button.transform.parent = transform;
			var rect = button.GetComponent<RectTransform>();
			buttonHeight = rect.rect.height;
			rect.anchoredPosition = new Vector2(0, -1 * (buttonHeight/2 + buttonHeight * i));
			button.gameObject.SetActive(true);
			var text = button.GetComponentInChildren<Text>();
			text.text = crew.name;

			i += 1;
		}
	}
	
	void Update () {

	}
}
