using UnityEngine;
using System.Collections;

public class PowerGenerator : MonoBehaviour {
	Ship ship;
	public int supplyRadius;
	public float outputRate;

	// Use this for initialization
	void Start () {
		ship = GetComponentInParent<Ship>();
	}
	
	// Update is called once per frame
	void Update () {
		foreach (var node in ship.GetBlockComponents<PowerNode>()) {
			if (Vector3.Distance(transform.position, node.transform.position) <= node.supplyRadius+supplyRadius) {
				if (node.charge < node.maxCharge)
					node.charge += outputRate*Time.deltaTime;
			}
		}
	}
}
