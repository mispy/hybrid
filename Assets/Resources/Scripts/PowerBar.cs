using UnityEngine;
using System.Collections;

public class PowerBar : MonoBehaviour {
	PowerNode powerNode;

	// Use this for initialization
	void Start () {
		powerNode = GetComponentInParent<PowerNode>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = new Vector3(transform.localScale.x, powerNode.charge / powerNode.maxCharge, transform.localScale.z);		
	}
}
