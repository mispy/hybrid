using UnityEngine;
using System.Collections;

public class PulseBullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider) {
		var crewBody = collider.gameObject.GetComponent<CrewBody>();

		if (crewBody != null) {
			crewBody.TakeDamage(10);
		}

		Pool.Recycle(this.gameObject);
	}
}
