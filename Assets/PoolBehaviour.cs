using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PoolBehaviour : NetworkBehaviour {
    public virtual void OnCreate() { }
    public virtual void OnRecycle() { }
    
    public void DestroyChildren() {
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }
}