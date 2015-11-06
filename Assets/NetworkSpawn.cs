using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkSpawn : NetworkBehaviour {    
    [SyncVar(hook="SyncParent")]
    public uint parentId = 0;

    public void Awake() {
        if (NetworkServer.active) {
            var netId = transform.parent.gameObject.GetComponent<NetworkIdentity>();
            if (netId != null)
                parentId = netId.netId.Value;
            NetworkServer.Spawn(this.gameObject);
        }
    }

    public override void OnStartClient() {
        SyncParent(parentId);
    }

    public void SyncParent(uint newId) {    
        parentId = newId;
        if (newId == 0 || NetworkServer.active) return;
        var parent = ClientScene.FindLocalObject(new NetworkInstanceId(newId));
        transform.SetParent(parent.transform);
    }
}
