using UnityEngine;
using System.Collections;

public class PowerBar : MonoBehaviour {
    PowerReserve powerReserve;

    // Use this for initialization
    void Start () {
        powerReserve = GetComponentInParent<PowerReserve>();
    }
    
    // Update is called once per frame
    void Update () {
        transform.localScale = new Vector3(transform.localScale.x, powerReserve.currentPower / powerReserve.maxPower, transform.localScale.z);        
    }
}
