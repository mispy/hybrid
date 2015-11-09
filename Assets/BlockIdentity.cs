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

    void Awake() {
        var form = ClientScene.FindLocalObject(formId).GetComponent<Blockform>();
        form.RegisterBlock(this);
    }
}
