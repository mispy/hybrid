using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipName : MonoBehaviour {
	public InputField field;

	void Awake() {
		field = GetComponent<InputField>();
		field.onEndEdit.AddListener(OnEndEdit);
	}

	void OnEndEdit(string text) {
		Game.playerShip.name = text;
		Game.UnblockInput();
	}

	void Update() {
		if (field.isFocused)
			Game.BlockInput("ShipName");

		field.text = Game.playerShip.name;
		field.enabled = true;
	}
}
