using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerProducer : BlockComponent {
    public int supplyRadius;
    PowerCircle circle;

    public override void OnCreate() {        
        circle = Pool.For("PowerCircle").Take<PowerCircle>();
        circle.transform.SetParent(transform);
        circle.transform.position = transform.position;
        circle.transform.rotation = transform.rotation;
        circle.renderer.enabled = false;
        circle.gameObject.SetActive(true);
    }
        
    public void OnBlockSelected() {
        circle.renderer.enabled = true;
    }
    
    public void OnBlockDeselected() {
        circle.renderer.enabled = false;
    }
}
