using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerProducer : BlockComponent {
    public int supplyRadius;
    public bool isProducing {
        get { return isSwitchedOn && hasAvailablePower; }
    }
    public bool isSwitchedOn = true;
    public bool hasAvailablePower = true;
    PowerCircle circle;

    Transform noPowerIndicator;

    
    public override void OnCreate() {        
        circle = Pool.For("PowerCircle").Attach<PowerCircle>(transform);
        circle.spriteRenderer.enabled = false;
    }
        
    public void OnBlockSelected() {
        circle.spriteRenderer.enabled = true;
    }
    
    public void OnBlockDeselected() {
        circle.spriteRenderer.enabled = false;
    }

    public void Update() {
        if (!isProducing && noPowerIndicator == null) {
            noPowerIndicator = Pool.For("NoPower").Attach<Transform>(transform);
            noPowerIndicator.transform.rotation = block.ship.transform.rotation;
            noPowerIndicator.gameObject.SetActive(true);

            block.gameObject.SendMessage("OnDepowered", SendMessageOptions.DontRequireReceiver);
        } else if (isProducing && noPowerIndicator != null) {
            Pool.Recycle(noPowerIndicator.gameObject);

            block.gameObject.SendMessage("OnPowered", SendMessageOptions.DontRequireReceiver);
        }
    }
}
