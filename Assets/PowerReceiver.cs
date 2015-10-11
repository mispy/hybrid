using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerReceiver : BlockComponent {
    public float consumeRate = 0.0f;
    [HideInInspector]
    public bool isReceiving;

    [HideInInspector]
    public List<PowerProducer> availableProducers = new List<PowerProducer>();
    [HideInInspector]
    private GameObject noPowerIndicator;

    public void Start() {
        isReceiving = true;
    }

    public void Powered() {
        if (block.isPowered == true) return;
        if (noPowerIndicator != null)
            Pool.Recycle(noPowerIndicator);
        block.isPowered = true;
        block.gameObject.SendMessage("OnPowered", SendMessageOptions.DontRequireReceiver);
    }

    public void Depowered() {
        if (block.isPowered == false) return;
        noPowerIndicator = Pool.For("NoPower").TakeObject();
        noPowerIndicator.transform.SetParent(transform);
        noPowerIndicator.transform.rotation = block.ship.form.transform.rotation;
        noPowerIndicator.transform.position = transform.position;
        noPowerIndicator.SetActive(true);
        block.isPowered = false;
        block.gameObject.SendMessage("OnDepowered", SendMessageOptions.DontRequireReceiver);
    }

    public bool IsPowered() {
        if (!isReceiving) return false;

        foreach (var producer in form.GetBlockComponents<PowerProducer>()) {
            if (producer.isProducing && IntVector2.Distance(block.pos, producer.block.pos) <= producer.supplyRadius) {
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
