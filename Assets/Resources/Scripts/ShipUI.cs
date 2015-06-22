using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipUI : MonoBehaviour {
	public static ShipUI main;
	public Text shipName;


	void Awake() {
		ShipUI.main = this;
	}

	void Update() {
		var ship = Crew.player.maglockShip;

		if (ship == null) {
			shipName.text = "";
			return;
		}

		shipName.text = ship.name;
	}
}
