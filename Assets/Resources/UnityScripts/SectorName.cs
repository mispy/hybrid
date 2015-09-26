using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectorName : MonoBehaviour {
	void OnEnable() {
		var text = GetComponent<Text>();
		text.text = "Sector " + Game.activeSector.sector.Id;
	}
}
