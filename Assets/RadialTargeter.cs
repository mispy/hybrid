using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RadialTargeter : BlockComponent {
	[HideInInspector]
	public Blockform form { get; private set; }
	[HideInInspector]
	public GameObject targetCircle { get; private set; }

	public GameObject targetCirclePrefab;
	public float radius;

	void Start() {
		form = GetComponentInParent<Blockform>();
		targetCircle = Pool.For(targetCirclePrefab).TakeObject();
	}

	public void Update() {
		if (form.ship != Game.playerShip || !(Game.main.weaponSelect.selectedType == block.type)) {
			targetCircle.SetActive(false);
			return;
		}

		targetCircle.SetActive(true);
		targetCircle.transform.localScale = new Vector3(radius*2, radius*2, radius*2);
		targetCircle.transform.position = Game.mousePos;
	}
}
