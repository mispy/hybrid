using UnityEngine;
using System.Collections;

public class ShipPowerManager : MonoBehaviour {
	Ship ship;

	// Use this for initialization
	void Start () {
		ship = GetComponent<Ship>();
	}
	
	// Update is called once per frame
	void Update () {
		foreach (var block in ship.blocks.FindType(BlockType.Generator)) {

		}
	}
}
