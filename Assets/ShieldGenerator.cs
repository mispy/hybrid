using UnityEngine;
using System.Collections;

public class ShieldGenerator : BlockComponent {
	public override void OnRealize() {
		if (form.shields == null) {
			//Pool.For("Shields").Attach<Shields>(form.transform);
		}
	}

    public void OnDepowered() {
        //form.shields.UpdateStatus();
    }

    public void OnPowered() {
        //form.shields.UpdateStatus();
    }
}