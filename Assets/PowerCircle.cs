using UnityEngine;
using System.Collections;

public class PowerCircle : PoolBehaviour {
    PowerProducer producer;
    [HideInInspector]
    public SpriteRenderer renderer;

    public override void OnCreate() {
        renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = false;
    }

    public void Awake() {
        producer = GetComponentInParent<PowerProducer>();
    }
    
    // Update is called once per frame
    void Update() {
        var supplyRadius = producer.supplyRadius;
        if (supplyRadius*3 != transform.localScale.x) {
            transform.localScale = new Vector3(supplyRadius*3, supplyRadius*3, 1);
        }
    }
}
