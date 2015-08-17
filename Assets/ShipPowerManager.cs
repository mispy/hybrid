using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPowerManager : MonoBehaviour {
	Ship ship;
	float timePassed = 0.0f;

	// Use this for initialization
	void Start () {
		ship = GetComponent<Ship>();
	}

	// Update is called once per frame
	void Update () {		
		timePassed += Time.deltaTime;

		if (timePassed > 0.5f) {
			UpdatePower(timePassed);
			timePassed = 0.0f;
		}
	}

	void UpdatePower(float deltaTime) {
		var receivers = ship.GetBlockComponents<PowerReceiver>();

		foreach (var generator in ship.GetBlockComponents<PowerGenerator>()) {
			var availablePower = generator.powerSupplyRate * deltaTime;

			foreach (var receiver in receivers) {
				if (IntVector3.Distance(generator.block.pos, receiver.block.pos) <= generator.powerSupplyRadius) {
					availablePower = receiver.TakePower(availablePower, deltaTime);
					if (availablePower <= 0) break;
				}
			}
		}

		/*foreach (var node in ship.GetBlockComponents<PowerNode>()) {
			foreach (var otherNode in ship.GetBlockComponents<PowerNode>()) {
				if (IntVector3.Distance(node.block.pos, otherNode.block.pos) <= node.powerSupplyRadius+otherNode.powerSupplyRadius) {
					var change = node.powerSupplyRate * deltaTime;
					if (node.charge > change && node.charge > otherNode.charge && node.charge - change >= otherNode.charge) {
						node.charge -= change;
						otherNode.charge += change;
					}
				}
			}
		}*/

		foreach (var receiver in receivers) {
			receiver.UpdatePower(deltaTime);
		}
	}
}
