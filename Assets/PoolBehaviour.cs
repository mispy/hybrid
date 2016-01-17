using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PoolBehaviour : MonoBehaviour {
    [HideInInspector]
    public GUID guid;
    [HideInInspector]
    public bool needsSync = false;
    [ReadOnlyAttribute]
    public float syncCountdown = 0f;
    [HideInInspector]
    public int channel = 0;
    public float syncRate = 0.1f;
    [HideInInspector]
    public SyncMessage lastSyncMessage;
    [HideInInspector]
    public float syncDeltaTime;
    [HideInInspector]
    public float lastSyncReceived = 0f;
    [HideInInspector]
    // Marked as true during execution of OnDeserialize       
    public bool deserializing = false;

    public bool hasAuthority {
        get {
            return ((Game.localPlayer != null && Game.localPlayer.gameObject == this.gameObject) || (GetComponent<CrewBody>() == null && SpaceNetwork.isServer));
        }
    }

    public virtual void OnCreate() { }
    public virtual void OnRecycle() { }

    public virtual void OnSerialize(MispyNetworkWriter writer, bool initial) { }
    public virtual void OnDeserialize(MispyNetworkReader reader, bool initial) { }


    public void DestroyChildren() {
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }
}