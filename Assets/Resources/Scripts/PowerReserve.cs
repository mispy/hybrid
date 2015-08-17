using UnityEngine;
using System.Collections;

public class PowerReserve : MonoBehaviour {
	public PowerReceiver powerReceiver;
	public GameObject powerCircle;
	public float currentPower = 0.0f;
	public float maxPower;

	// Use this for initialization
	void Start () {
		powerReceiver = GetComponent<PowerReceiver>();
		powerCircle = GetComponentInChildren<PowerCircle>().gameObject;
		powerReceiver.OnPowerUpdate += OnPowerUpdate;
	}

	void OnPowerUpdate(float power) {
		currentPower = Mathf.Min(maxPower, currentPower + power);
		powerReceiver.powerConsumeRate = Mathf.Min(10f, maxPower-currentPower);

		if (currentPower <= 0.0f) {
			powerCircle.SetActive(false);
		} else {
			powerCircle.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
