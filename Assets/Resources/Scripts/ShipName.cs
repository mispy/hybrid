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
		Crew.player.maglockShip.name = text;
		Game.UnblockInput();
	}

	void Update() {
		if (field.isFocused)
			Game.BlockInput("ShipName");

		if (Crew.player.maglockShip == null) {
			field.text = "";
			field.enabled = false;
		} else if (!field.isFocused) {
			field.text = Crew.player.maglockShip.name;
			field.enabled = true;
		}
	}
}
