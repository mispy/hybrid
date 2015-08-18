using UnityEngine;
using System.Collections;

public class PowerNode : BlockType {
	public PowerProducer producer;
	public PowerReserve reserve;
	public PowerReceiver receiver;
	public PowerCircle circle;

	private float defaultSupplyRate;
	private float defaultConsumeRate;

	public void Start() {
		producer = GetComponent<PowerProducer>();
		reserve = GetComponent<PowerReserve>();
		receiver = GetComponent<PowerReceiver>();
		circle = GetComponentInChildren<PowerCircle>();

		producer.OnPowerTaken += OnPowerTaken;
		receiver.OnPowerAdded += OnPowerAdded;

		circle.gameObject.SetActive(false);
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
		reserve.currentPower = Mathf.Min(reserve.maxPower, reserve.currentPower + amount);
		if (reserve.currentPower >= reserve.maxPower/5) {
			producer.supplyRate = reserve.currentPower;
			circle.gameObject.SetActive(true);
		}

		if (reserve.currentPower == reserve.maxPower) {
			receiver.consumeRate = 0.0f;
		}
	}

	public void OnDestroy() {
		producer.OnPowerTaken -= OnPowerTaken;
		receiver.OnPowerAdded -= OnPowerAdded;
	}
}
