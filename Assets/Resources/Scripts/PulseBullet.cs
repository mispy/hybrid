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
		var crew = collider.gameObject.GetComponent<Crew>();

		if (crew != null) {
			crew.TakeDamage(10);
		}

		Pool.Recycle(this.gameObject);
	}
}
