using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipPowerManager : MonoBehaviour {
	Ship ship;

	// Use this for initialization
	void Start () {
		ship = GetComponent<Ship>();
		ship.blocks.OnBlockChanged += OnBlockChanged;
	}
	
	public void OnBlockChanged(Block newBlock, Block oldBlock) {
	}

	// Update is called once per frame
	void Update () {		
		foreach (var generator in ship.GetBlockComponents<PowerGenerator>()) {
			foreach (var node in ship.GetBlockComponents<PowerNode>()) {
				if (IntVector2.Distance(generator.block.pos, node.block.pos) <= generator.powerSupplyRadius+node.powerSupplyRadius) {
					if (node.charge < node.maxCharge)
						node.charge += generator.powerSupplyRate*Time.deltaTime;
				}

			}
		}

		foreach (var node in ship.GetBlockComponents<PowerNode>()) {
			foreach (var otherNode in ship.GetBlockComponents<PowerNode>()) {
				if (IntVector2.Distance(node.block.pos, otherNode.block.pos) <= node.powerSupplyRadius+otherNode.powerSupplyRadius) {
					var change = node.powerSupplyRate * Time.deltaTime;
					if (node.charge > change && node.charge > otherNode.charge && node.charge - change >= otherNode.charge) {
						node.charge -= change;
						otherNode.charge += change;
					}
				}
			}
		}
	}
}
