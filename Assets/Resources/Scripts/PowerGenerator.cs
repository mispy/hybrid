using UnityEngine;
using System.Collections;

public class PowerGenerator : MonoBehaviour {
	Ship ship;
	public Block block;
	public int powerSupplyRadius;
	public float powerSupplyRate;

	// Use this for initialization
	void Start () {
		ship = GetComponentInParent<Ship>();
		block = ship.BlockAtWorldPos(transform.position);
	}
}
