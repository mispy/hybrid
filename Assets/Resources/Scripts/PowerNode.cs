using UnityEngine;
using System.Collections;

public class PowerNode : MonoBehaviour {
	public Block block;
	public int powerSupplyRadius;
	public float powerSupplyRate;
	public float maxCharge;

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
		block = ship.BlockAtWorldPos(transform.position);
	}
}
