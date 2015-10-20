using UnityEngine;
using System.Collections;

public class PowerNode : BlockComponent {
    PowerProducer producer;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
    }

    public void OnPowerUpdate() {
        producer.hasAvailablePower = false;

        foreach (var reactor in form.GetBlockComponents<Reactor>()) {
            if (reactor.producer.isProducing) {
                producer.hasAvailablePower = true;
                break;
            }
        }
    }
}
