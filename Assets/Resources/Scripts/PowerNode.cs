using UnityEngine;
using System.Collections;

public class PowerNode : BlockType {
	public PowerProducer producer;
	public PowerReserve reserve;
	public PowerReceiver receiver;
	public PowerCircle circle;

	public void Start() {
		producer = GetComponent<PowerProducer>();
		reserve = GetComponent<PowerReserve>();
		receiver = GetComponent<PowerReceiver>();
		circle = GetComponentInChildren<PowerCircle>();

		producer.OnPowerTaken += OnPowerTaken;
		receiver.OnPowerAdded += OnPowerAdded;
		reserve.currentPower = reserve.maxPower/2;
	}

	public void OnPowerTaken(float amount) {
		//Debug.LogFormat("Taken: {0} of {1}", amount, reserve.currentPower);
		reserve.currentPower = Mathf.Max(0.0f, reserve.currentPower - amount);
		if (reserve.currentPower < reserve.maxPower/3) {
			producer.supplyRate = 0.0f;
			circle.gameObject.SetActive(false);
		}

		if (reserve.currentPower < reserve.maxPower) {
			receiver.consumeRate = 10.0f;
			producer.supplyRate = 10.0f;
		}
	}

	public void OnPowerAdded(float amount) {
		//Debug.LogFormat("Added: {0} to {1}", amount, reserve.currentPower);
		reserve.currentPower = Mathf.Min(reserve.maxPower, reserve.currentPower + amount);
		if (reserve.currentPower >= reserve.maxPower/3) {
			producer.supplyRate = 10.0f;
			circle.gameObject.SetActive(true);
		}

		if (reserve.currentPower == reserve.maxPower) {
			receiver.consumeRate = 0.0f;
			producer.supplyRate = 20.0f;
		}
	}

	public void OnDestroy() {
		producer.OnPowerTaken -= OnPowerTaken;
		receiver.OnPowerAdded -= OnPowerAdded;
	}
}
