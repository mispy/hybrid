using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipPowerManager : MonoBehaviour {
    Blockform ship;

    // Use this for initialization
    void Start () {
        ship = GetComponent<Blockform>();
        ship.blocks.OnBlockRemoved += OnBlockChange;
        ship.blocks.OnBlockAdded += OnBlockChange;
        OnBlockChange(null);
    }

    List<PowerProducer> producers;
    List<PowerReceiver> receivers;
    
    string[] producePriority = {
        "PowerNode"
    };

    string[] receiveLast = {
        "PowerNode"
    };

    void OnBlockChange(Block block) {
        producers = ship.GetBlockComponents<PowerProducer>().ToList();
        receivers = ship.GetBlockComponents<PowerReceiver>().ToList();

        producers = producers.OrderBy((producer) => -Array.IndexOf(producePriority, producer.block.type.name)).ToList();
        receivers = receivers.OrderBy((receiver) => Array.IndexOf(receiveLast, receiver.block.type.name)).ToList();

        foreach (var receiver in receivers) {
            receiver.availableProducers.Clear();
        }
        
        foreach (var producer in producers) {
            foreach (var receiver in receivers) {
                if (producer.gameObject == receiver.gameObject)
                    continue;

                if (IntVector2.Distance(producer.block.pos, receiver.block.pos) <= producer.supplyRadius) {
                    receiver.availableProducers.Add(producer);
                }
            }
        }
    }

    void UpdatePower(float deltaTime) {        
        foreach (var receiver in receivers) {
            foreach (var producer in receiver.availableProducers) {
                if (!producer.CanGivePower(receiver)) {
                    producer.forbiddenReceivers.Add(receiver);
                }
            }
        }

        foreach (var producer in producers) {
            producer.availablePower = producer.supplyRate*deltaTime;
        }    

        foreach (var receiver in receivers) {
            var powerNeeded = receiver.consumeRate*deltaTime;
            var availablePower = 0.0f;
            foreach (var producer in receiver.availableProducers) {
                if (!producer.forbiddenReceivers.Contains(receiver))
                    availablePower += producer.availablePower;
            }

            if (availablePower < powerNeeded && !receiver.isDynamic) {
                receiver.Depowered();
                continue;
            } else {
                var powerTaken = 0.0f;
                foreach (var producer in receiver.availableProducers) {
                    if (producer.forbiddenReceivers.Contains(receiver))
                        continue;

                    var toTake = Mathf.Min(producer.availablePower, powerNeeded);
                    powerNeeded -= producer.TransferPower(receiver, toTake);
                    if (powerNeeded <= 0.0f) break;
                }
                receiver.Powered();
            }
        }

        foreach (var producer in producers) {
            producer.forbiddenReceivers.Clear();
        }
    }

    public float counter = 0.0f;
    void Update() {
        counter += Time.deltaTime;
        if (counter >= 0.2f) {
            UpdatePower(counter);
            counter = 0.0f;
        }
    }
}
