using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explosion : PoolBehaviour
{

    public void Cleanup() {
        Pool.Recycle(gameObject);
    }
}