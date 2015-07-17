using UnityEngine;
using System.Collections;

public class PowerNode : MonoBehaviour {
	public int supplyRadius = 10;
	public float maxCharge = 100f;
	public float outputRate = 10f;

	float _charge = 10f;
	public float charge {
		get { 
			return _charge;
		}
		set { 
			_charge = value;
			if (_charge > maxCharge)
				_charge = maxCharge;
			if (_charge < 0)
				_charge = 0;
		}
	}

	Ship ship;

	void Start () {
		ship = GetComponentInParent<Ship>();
	}

	void Update() {
		foreach (var node in ship.GetBlockComponents<PowerNode>()) {
			if (Vector3.Distance(transform.position, node.transform.position) <= node.supplyRadius+supplyRadius) {
				node.charge += outputRate * Time.deltaTime;
			}
		}
	}
}
