using UnityEngine;
using System.Collections;

public class ShieldGenerator : BlockType {
	void Awake() {
		var form = GetComponentInParent<Blockform>();
		if (form.shields == null) {
			var shields = Pool.For("Shields").Take<Shields>();
			shields.transform.SetParent(form.transform);
			shields.gameObject.SetActive(true);
		}
	}
}