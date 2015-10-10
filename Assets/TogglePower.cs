using UnityEngine;
using System.Collections;

public class TogglePower : BlockAbility {
    public override bool WorksWith(Block block) {
        return block.type.GetComponent<PowerReceiver>() != null;
    }

	void OnEnable() {
	    foreach (var block in blocks) {
            var receiver = block.gameObject.GetComponent<PowerReceiver>();
            receiver.isReceiving = !receiver.isReceiving;
        }    

        gameObject.SetActive(false);
	}
}
