using UnityEngine;
using System.Collections;

public class PowerNode : BlockComponent {
    PowerProducer producer;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
    }

    public void Update() {
        producer.hasAvailablePower = false;

        foreach (var reactor in form.blocks.Find<Reactor>()) {
            var reactorProducer = reactor.gameObject.GetComponent<PowerProducer>();
            if (reactorProducer.isProducing)
                producer.hasAvailablePower = true;
                break;
        }
    }
}
