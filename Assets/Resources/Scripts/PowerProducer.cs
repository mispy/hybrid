using UnityEngine;
using System.Collections;

public class PowerProducer : BlockComponent {
	public int supplyRadius;
	public float supplyRate;

	public delegate void PowerTakenHandler(float amount);
	public event PowerTakenHandler OnPowerTaken = delegate { };

	[HideInInspector]
	public float availablePower;

	public float TakePower(float amount) {
		availablePower -= amount;
		OnPowerTaken(amount);
		return amount;
	}
}
