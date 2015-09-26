﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerReceiver : BlockComponent {
    public float consumeRate = 0.0f;
    [HideInInspector]
    public bool isPowered;

    public bool isDynamic;

    public delegate void PowerAddedHandler(PowerProducer producer, float power);
    public event PowerAddedHandler OnPowerAdded = delegate { };

    [HideInInspector]
    public List<PowerProducer> availableProducers = new List<PowerProducer>();
    [HideInInspector]
    private GameObject noPowerIndicator;

    public void Start() {
        isPowered = true;
    }

    public void ReceivePower(PowerProducer producer, float amount) {
        OnPowerAdded(producer, amount);
    }

    public void Powered() {
        if (isPowered == true) return;
        if (noPowerIndicator != null)
            Pool.Recycle(noPowerIndicator);
        isPowered = true;
    }

    public void Depowered() {
        if (isPowered == false) return;
        noPowerIndicator = Pool.For("NoPower").TakeObject();
        noPowerIndicator.transform.SetParent(block.gameObject.transform);
        noPowerIndicator.transform.rotation = block.ship.form.transform.rotation;
        noPowerIndicator.transform.position = block.gameObject.transform.position;
        noPowerIndicator.SetActive(true);
        isPowered = false;
    }
}