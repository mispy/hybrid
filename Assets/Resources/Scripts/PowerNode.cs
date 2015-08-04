using UnityEngine;
using System.Collections;

public class PowerNode : MonoBehaviour {
	public int supplyRadius;
	public float maxCharge;
	public float outputRate;

	float _charge = 0f;
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
				var change = outputRate * Time.deltaTime;
				if (charge > change && charge > node.charge && charge - change >= node.charge) {
					this.charge -= change;
					node.charge += change;
				}
			}
		}
	}
}
