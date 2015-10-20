using UnityEngine;
using System.Collections;

public class Reactor : BlockComponent {
    public PowerProducer producer;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
    }

    public void OnDepowered() {
        foreach (var node in form.GetBlockComponents<PowerNode>()) {
            node.OnPowerUpdate();
        }
    }

    public void OnPowered() {
        foreach (var node in form.GetBlockComponents<PowerNode>()) {
            node.OnPowerUpdate();
        }
    }
}
