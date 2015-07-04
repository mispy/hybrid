using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ResourcePanel : MonoBehaviour {
	public Text scrapAmount;
	
	void Update() {
		if (Crew.player.maglockShip != null) {
			var ship = Crew.player.maglockShip;
			scrapAmount.text = String.Format("{0}/1000", Mathf.RoundToInt(ship.scrapAvailable));
		}
	}
}
