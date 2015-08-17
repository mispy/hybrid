using UnityEngine;
using System.Collections;

public class PowerReceiver : BlockComponent {
	public float powerConsumeRate = 0.0f;
	[HideInInspector]
	public float powerAvailableThisUpdate = 0.0f;
	[HideInInspector]
	public bool isPowered;

	public delegate void PowerReceiveHandler(float power);
	public event PowerReceiveHandler OnPowerUpdate = delegate { };

	private GameObject noPowerIndicator;

	public void Start() {
		isPowered = true;
	}

	public float TakePower(float availablePower, float deltaTime) {
		var powerTaken = Mathf.Min(availablePower, powerConsumeRate*deltaTime - powerAvailableThisUpdate);
		powerAvailableThisUpdate += powerTaken;
		return availablePower - powerTaken;
	}

	public void UpdatePower(float deltaTime) {
		//Debug.LogFormat("{0} {1} {2} {3}", block.type.name, powerConsumeRate*deltaTime, powerAvailableThisUpdate, isPowered);
		if (powerAvailableThisUpdate >= powerConsumeRate*deltaTime && isPowered == false) {
			if (noPowerIndicator != null)
				Pool.Recycle(noPowerIndicator);
			isPowered = true;
		} else if (powerAvailableThisUpdate < powerConsumeRate*deltaTime && isPowered == true) {
			noPowerIndicator = Pool.For("NoPower").TakeObject();
			noPowerIndicator.transform.SetParent(block.gameObject.transform);
			noPowerIndicator.transform.rotation = block.ship.transform.rotation;
			noPowerIndicator.transform.position = block.gameObject.transform.position;
			noPowerIndicator.SetActive(true);
			isPowered = false;
		}

		OnPowerUpdate(powerAvailableThisUpdate);
		powerAvailableThisUpdate = 0.0f;
	}
}
