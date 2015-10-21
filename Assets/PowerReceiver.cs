using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerReceiver : BlockComponent {
    public float consumeRate = 0.0f;
    [HideInInspector]
    public bool isReceiving;

    [HideInInspector]
    public PowerProducer attachedProducer;

    [HideInInspector]
    private Transform noPowerIndicator;

    public void Start() {
        isReceiving = true;
        block.isPowered = false;
    }
        
    public override void OnRecycle() {
        if (block.type.isWeapon)
            form.poweredWeapons.Remove(block);
    }

    public void Powered() {
        if (block.isPowered == true) return;
        if (noPowerIndicator != null)
            Pool.Recycle(noPowerIndicator.gameObject);

        block.isPowered = true;
        if (block.type.isWeapon)
            form.poweredWeapons.Add(block);

        block.gameObject.SendMessage("OnPowered", SendMessageOptions.DontRequireReceiver);
    }

    public void Depowered() {
        if (block.isPowered == false) return;
        noPowerIndicator = Pool.For("NoPower").Attach<Transform>(transform);
        noPowerIndicator.transform.rotation = block.ship.form.transform.rotation;
        block.isPowered = false;
        if (block.type.isWeapon)
            form.poweredWeapons.Remove(block);

        block.gameObject.SendMessage("OnDepowered", SendMessageOptions.DontRequireReceiver);
    }

    public bool IsPowered() {
        if (!isReceiving) return false;
        if (attachedProducer != null && attachedProducer.isActiveAndEnabled && attachedProducer.isProducing) 
            return true;

        foreach (var producer in form.GetBlockComponents<PowerProducer>()) {
            if (producer.isProducing && IntVector2.Distance(block.pos, producer.block.pos) <= producer.supplyRadius) {
                attachedProducer = producer;
                return true;
            }
        }

        return false;
    }

    public void Update() {
        var powered = IsPowered();

        if (powered && !block.isPowered)
            Powered();
        else if (!powered && block.isPowered)
            Depowered();
    }
}
