using UnityEngine;
using System.Collections;

public class ShieldGenerator : BlockComponent {
	void Awake() {
		if (form.shields == null) {
			var shields = Pool.For("Shields").Take<Shields>();
			shields.transform.SetParent(form.transform);
			shields.gameObject.SetActive(true);
		}
	}

    public void OnDepowered() {
        form.shields.UpdateStatus();
    }

    public void OnPowered() {
        form.shields.UpdateStatus();
    }
}