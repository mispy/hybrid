using UnityEngine;
using System.Collections;

public class PowerNode : BlockComponent {
    public PowerProducer producer;
    public PowerReserve reserve;
    public PowerReceiver receiver;
    public PowerCircle circle;

    private float defaultSupplyRate;
    private float defaultConsumeRate;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
        reserve = GetComponent<PowerReserve>();
        receiver = GetComponent<PowerReceiver>();

        circle = Pool.For("PowerCircle").Take<PowerCircle>();
        circle.transform.SetParent(transform);
        circle.transform.position = transform.position;
        circle.transform.rotation = transform.rotation;
        circle.renderer.enabled = false;
        circle.gameObject.SetActive(true);

        producer.OnPowerTaken += OnPowerTaken;
        receiver.OnPowerAdded += OnPowerAdded;
    }

    public void OnBlockSelected() {
        circle.renderer.enabled = true;
    }

    public void OnBlockDeselected() {
        circle.renderer.enabled = false;
    }

    public void OnPowerTaken(PowerReceiver otherReceiver, float amount) {
        //Debug.LogFormat("Taken: {0} of {1}", amount, reserve.currentPower);
        reserve.currentPower = Mathf.Max(0.0f, reserve.currentPower - amount);
        if (reserve.currentPower <= reserve.maxPower/5) {
            producer.supplyRate = 0.0f;
            circle.gameObject.SetActive(false);
        }

        if (reserve.currentPower < reserve.maxPower) {
            receiver.consumeRate = reserve.maxPower - reserve.currentPower;
        }
    }

    public void OnPowerAdded(PowerProducer otherProducer, float amount) {
        //Debug.LogFormat("Added: {0} to {1}", amount, reserve.currentPower);
        //reserve.currentPower = Mathf.Min(reserve.maxPower, reserve.currentPower + amount);
		reserve.currentPower = reserve.maxPower;
        //if (reserve.currentPower >= reserve.maxPower/5) {
            producer.supplyRate = reserve.currentPower;
            circle.gameObject.SetActive(true);
        //}

        if (reserve.currentPower == reserve.maxPower) {
            receiver.consumeRate = 0.0f;
        }
    }
}
