using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkSpawn : NetworkBehaviour {
    public void Awake() {
        if (Network.isServer)
            NetworkServer.Spawn(this.gameObject);
    }


    
    [SyncVar(hook="SyncParent")]
    public NetworkInstanceId parentId;
    
    public void SyncParent(NetworkInstanceId parentId) {
        var parent = ClientScene.FindLocalObject(parentId);
        Debug.Log(parent);
        transform.SetParent(parent.transform);
    }
}
