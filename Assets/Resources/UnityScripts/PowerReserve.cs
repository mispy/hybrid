using UnityEngine;
using System.Collections;

public class PowerReserve : BlockComponent {
	[HideInInspector]
	public PowerReceiver powerReceiver;
	public float currentPower = 0.0f;
	public float maxPower;

	// Use this for initialization
	void Start () {
		powerReceiver = GetComponent<PowerReceiver>();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
