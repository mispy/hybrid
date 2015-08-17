using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPowerManager : MonoBehaviour {
	Ship ship;

	// Use this for initialization
	void Start () {
		ship = GetComponent<Ship>();
	}

	// Update is called once per frame
	void Update () {		
		UpdatePower();
	}

	void UpdatePower() {
		var receivers = ship.GetBlockComponents<PowerReceiver>();

		foreach (var generator in ship.GetBlockComponents<PowerGenerator>()) {
			var availablePower = generator.powerSupplyRate * Time.deltaTime;

			foreach (var receiver in receivers) {
				if (IntVector3.Distance(generator.block.pos, receiver.block.pos) <= generator.powerSupplyRadius) {
					availablePower = receiver.TakePower(availablePower);
					if (availablePower <= 0) break;
				}
			}
		}

		foreach (var node in ship.GetBlockComponents<PowerNode>()) {
			foreach (var otherNode in ship.GetBlockComponents<PowerNode>()) {
				if (IntVector3.Distance(node.block.pos, otherNode.block.pos) <= node.powerSupplyRadius+otherNode.powerSupplyRadius) {
					var change = node.powerSupplyRate * Time.deltaTime;
					if (node.charge > change && node.charge > otherNode.charge && node.charge - change >= otherNode.charge) {
						node.charge -= change;
						otherNode.charge += change;
					}
				}
			}
		}

		foreach (var receiver in receivers) {
			receiver.UpdatePower();
		}
	}
}
