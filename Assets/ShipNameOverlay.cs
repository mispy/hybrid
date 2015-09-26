﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipNameOverlay : MonoBehaviour {
	Ship ship;
	Text text;
	Canvas canvas;

	// Use this for initialization
	void Start () {
		canvas = GetComponentInParent<Canvas>();
		ship = GetComponentInParent<Blockform>().ship;
		text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		var friendlyColor = new Color(155/255f, 251/255f, 152/255f);
		var hostileColor = new Color(255/255f, 69/255f, 0f);

		if (Game.playerShip.faction.IsEnemy(ship.faction)) {
			text.color = hostileColor;
		} else {
			text.color = friendlyColor;
		}

		text.fontSize = Mathf.CeilToInt(Mathf.Log(ship.blocks.size)*0.5f);

		canvas.transform.rotation = Game.mainCamera.transform.rotation;
		text.text = ship.name;
	}
}
