using UnityEngine;
using System.Collections;

public class PowerCircle : MonoBehaviour {
	PowerNode powerNode;
	// Use this for initialization
	void Start () {
		powerNode = GetComponentInParent<PowerNode>();
	}
	
	// Update is called once per frame
	void Update () {
		return;
		if (powerNode.supplyRadius != transform.localScale.x) {
			transform.localScale = new Vector3(powerNode.supplyRadius, powerNode.supplyRadius, 1);
		}
	}
}
