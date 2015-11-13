using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(NetworkIdentity))]
public class BlockIdentity : NetworkBehaviour {
    [SyncVar]
    public IntVector2 pos;
    [SyncVar]
    public BlockLayer layer;
    [SyncVar]
    public NetworkInstanceId formId;

    public override void OnStartClient() {
        Debug.Log("OnStartClient");
        var form = ClientScene.FindLocalObject(formId).GetComponent<Blockform>();
        form.RealizeBlock(this);
    }
}
