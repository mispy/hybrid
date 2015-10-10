using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipPowerManager : MonoBehaviour {
    Blockform ship;

    // Use this for initialization
    void Start () {
        ship = GetComponent<Blockform>();
    }
}
