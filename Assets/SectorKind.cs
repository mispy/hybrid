using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SectorKind : MonoBehaviour {
    public Jumpable beaconType;

    public SectorBounds bounds {
        get {
            return GetComponentInChildren<SectorBounds>();
        }
    }

    public void OnEnable() {
    }
}
