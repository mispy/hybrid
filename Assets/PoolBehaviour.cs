using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PoolBehaviour : MonoBehaviour {
    [HideInInspector]
    public GUID guid;
    [HideInInspector]
    public bool needsSync = false;
    [HideInInspector]
    public float syncCountdown = 0f;

    public virtual void OnCreate() { }
    public virtual void OnRecycle() { }

    public virtual void OnSerialize(ExtendedBinaryWriter writer, bool initial) { }
    public virtual void OnDeserialize(ExtendedBinaryReader reader, bool initial) { }


    public void DestroyChildren() {
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }
}