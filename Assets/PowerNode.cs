using UnityEngine;
using System.Collections;

public class PowerNode : BlockComponent {
    public PowerProducer producer;
    public PowerReserve reserve;
    public PowerReceiver receiver;
    public PowerCircle circle;

    private float defaultSupplyRate;
    private float defaultConsumeRate;

    public void Awake() {
        producer = GetComponent<PowerProducer>();
        reserve = GetComponent<PowerReserve>();
        receiver = GetComponent<PowerReceiver>();

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
