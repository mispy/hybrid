using UnityEngine;
using System.Collections;

public class ShieldGenerator : BlockType {
	void Awake() {
		var form = GetComponentInParent<Blockform>();
		var shields = form.GetComponentInChildren<Shields>();

		if (shields == null) {
			shields = Pool.For("Shields").Take<Shields>();
			shields.transform.SetParent(form.transform);
			shields.gameObject.SetActive(true);
		}
	}
}