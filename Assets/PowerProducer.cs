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
}
