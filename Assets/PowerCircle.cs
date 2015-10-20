using UnityEngine;
using System.Collections;

public class PowerCircle : PoolBehaviour {
    PowerProducer producer;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    public override void OnCreate() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    public void Awake() {
        producer = GetComponentInParent<PowerProducer>();
    }
    
    // Update is called once per frame
    void Update() {
        var supplyRadius = producer.supplyRadius;
        if (!producer.isProducing)
            supplyRadius = 0;

        if (supplyRadius != transform.localScale.x) {
            transform.localScale = new Vector3(supplyRadius, supplyRadius, 1);
        }
    }
}
