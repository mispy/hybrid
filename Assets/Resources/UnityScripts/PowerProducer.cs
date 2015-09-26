using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerProducer : BlockComponent {
    public int supplyRadius;
    public float supplyRate;

    public delegate void PowerTakenHandler(PowerReceiver receiver, float amount);
    public event PowerTakenHandler OnPowerTaken = delegate { };

    [HideInInspector]
    public float availablePower;
    [HideInInspector]
    public List<PowerReceiver> forbiddenReceivers = new List<PowerReceiver>();

    public override void OnCreate() {
        var obj = Pool.For("PowerCircle").TakeObject();
        obj.transform.SetParent(transform);
        obj.SetActive(true);
    }

    public bool CanGivePower(PowerReceiver receiver) {
        var srcReserve = GetComponent<PowerReserve>();
        var destReserve = receiver.GetComponent<PowerReserve>();

        if (srcReserve != null && destReserve != null && destReserve.currentPower >= srcReserve.currentPower) {
            return false;
        } else {
            return true;
        }
    }

    public float TransferPower(PowerReceiver receiver, float amount) {
        availablePower -= amount;
        //Debug.LogFormat("PowerTransfer: {0} => {1} ({2}) [{3} remaining]", this.name, receiver.name, amount, availablePower);
        OnPowerTaken(receiver, amount);
        receiver.ReceivePower(this, amount);
        return amount;
    }
}
