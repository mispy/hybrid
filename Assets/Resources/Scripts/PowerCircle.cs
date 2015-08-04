using UnityEngine;
using System.Collections;

public class PowerCircle : MonoBehaviour {
	PowerNode powerNode;
	PowerGenerator powerGenerator;

	// Use this for initialization
	void Start() {
		powerNode = GetComponentInParent<PowerNode>();
		powerGenerator = GetComponentInParent<PowerGenerator>();
	}
	
	// Update is called once per frame
	void Update() {
		var supplyRadius = (powerNode == null ? powerGenerator.supplyRadius : powerNode.supplyRadius);
		if (supplyRadius != transform.localScale.x) {
			transform.localScale = new Vector3(supplyRadius*3, supplyRadius*3, 1);
		}
	}
}
