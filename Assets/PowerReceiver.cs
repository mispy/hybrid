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
    }

    public void Depowered() {
        if (block.isPowered == false) return;
        noPowerIndicator = Pool.For("NoPower").TakeObject();
        noPowerIndicator.transform.SetParent(block.gameObject.transform);
        noPowerIndicator.transform.rotation = block.ship.form.transform.rotation;
        noPowerIndicator.transform.position = block.gameObject.transform.position;
        noPowerIndicator.SetActive(true);
        block.isPowered = false;
    }

    public void Update() {
        var powered = true;

        if (isReceiving == false)
            powered = false;

        if (powered && !block.isPowered)
            Powered();
        else if (!powered && block.isPowered)
            Depowered();
    }
}
