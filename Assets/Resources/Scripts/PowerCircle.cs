using UnityEngine;
using System.Collections;

public class PowerCircle : MonoBehaviour {
	PowerProducer producer;

	// Use this for initialization
	void Start() {
		producer = GetComponentInParent<PowerProducer>();
	}
	
	// Update is called once per frame
	void Update() {
		var supplyRadius = producer.supplyRadius;
		if (supplyRadius*3 != transform.localScale.x) {
			transform.localScale = new Vector3(supplyRadius*3, supplyRadius*3, 1);
		}
	}
}
