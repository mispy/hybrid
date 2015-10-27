using UnityEngine;
using System.Collections;

public class Beacon : MonoBehaviour {
    public static Beacon Create(Star star) {
        var beacon = Pool.For("Beacon").Attach<Beacon>(star.transform);
        beacon.transform.localPosition = star.BeaconPosition();
        return beacon;
    }
}
