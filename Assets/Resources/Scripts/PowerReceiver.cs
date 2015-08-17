using UnityEngine;
using System.Collections;

public class PowerReceiver : BlockComponent {
	public float powerConsumeRate = 0.0f;
	[HideInInspector]
	public float powerAvailableThisUpdate = 0.0f;
	[HideInInspector]
	public bool isPowered;

	private GameObject noPowerIndicator;

	public void Start() {
		isPowered = true;
		UpdatePower();
	}

	public float TakePower(float availablePower) {
		var powerTaken = Mathf.Min(availablePower, powerConsumeRate - powerAvailableThisUpdate);
		powerAvailableThisUpdate += powerTaken;
		return availablePower - powerTaken;
	}

	public void UpdatePower() {
		if (powerAvailableThisUpdate >= powerConsumeRate && isPowered == false) {
			if (noPowerIndicator != null)
				Pool.Recycle(noPowerIndicator);
			isPowered = true;
		} else if (powerAvailableThisUpdate < powerConsumeRate && isPowered == true) {
			noPowerIndicator = Pool.For("NoPower").TakeObject();
			noPowerIndicator.transform.SetParent(block.gameObject.transform);
			noPowerIndicator.transform.rotation = block.ship.transform.rotation;
			noPowerIndicator.transform.position = block.gameObject.transform.position;
			noPowerIndicator.SetActive(true);
			isPowered = false;
		}

		powerAvailableThisUpdate = 0.0f;
	}
}
