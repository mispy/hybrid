using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipPowerManager : MonoBehaviour {
	Ship ship;

	// Use this for initialization
	void Start () {
		ship = GetComponent<Ship>();
		ship.blocks.OnBlockAdded += OnBlockChange;
		ship.blocks.OnBlockRemoved += OnBlockChange;
		OnBlockChange(null);
	}

	List<PowerProducer> producers;
	List<PowerReceiver> receivers;

	void OnBlockChange(Block block) {
		producers = ship.GetBlockComponents<PowerProducer>().ToList();
		receivers = ship.GetBlockComponents<PowerReceiver>().ToList();

		foreach (var receiver in receivers) {
			receiver.availableProducers.Clear();
		}
		
		foreach (var producer in producers) {
			foreach (var receiver in receivers) {
				if (producer.gameObject == receiver.gameObject)
					continue;

				if (IntVector3.Distance(producer.block.pos, receiver.block.pos) <= producer.supplyRadius) {
					receiver.availableProducers.Add(producer);
				}
			}
		}
	}

	void UpdatePower(float deltaTime) {		
		foreach (var producer in producers) {
			producer.availablePower = producer.supplyRate*deltaTime;
		}

		foreach (var receiver in receivers) {
			var powerNeeded = receiver.consumeRate*deltaTime;
			var availablePower = 0.0f;
			foreach (var producer in receiver.availableProducers) {
				availablePower += producer.availablePower;
			}

			if (availablePower < powerNeeded) {
				receiver.Depowered();
				continue;
			} else {
				var powerTaken = 0.0f;
				foreach (var producer in receiver.availableProducers) {
					var toTake = Mathf.Min(producer.availablePower, powerNeeded - powerTaken);
					powerTaken += producer.TakePower(toTake);
					if (powerTaken >= powerNeeded) break;
				}
				receiver.Powered(powerNeeded);
			}
		}
	}

	void Update() {
		UpdatePower(Time.deltaTime);
	}
}
