using UnityEngine;
using System.Collections;

public class PowerNode : BlockComponent {
    PowerProducer producer;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
    }
}
