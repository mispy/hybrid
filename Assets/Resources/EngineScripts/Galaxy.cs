using UnityEngine;
using System.Collections;

public class Galaxy {
	public void Simulate(float deltaTime) {
		foreach (var ship in ShipManager.all) {
			ship.Simulate(deltaTime);
		}
	}
}
