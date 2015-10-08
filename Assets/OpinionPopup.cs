using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum OpinionEvent {
	ShipRouted
}

public class OpinionPopup : MonoBehaviour {
	Text text;

	// Use this for initialization
	void Start () {
		text = GetComponentInChildren<Text>();
	}

	public void ApprovalChange(Faction faction, int amount) {
		if (amount < 0) {
			text.text += String.Format("\n<color=red>{1} {0} Disapproves</color>", faction.name, amount);
		} else {
			text.text += String.Format("\n<color=lime>+{1} {0} Approves</color>", faction.name, amount);
		}
	}

	public void OnShipRouted(Ship ship) {
		text.text += String.Format("\n<size=16><color=cyan>{0} - Routed</color></size>", ship.form.name);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
