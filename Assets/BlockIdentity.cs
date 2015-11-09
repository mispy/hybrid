using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class BlockIdentity : NetworkBehaviour {
    [SyncVar]
    public IntVector2 pos;
    [SyncVar]
    public BlockLayer layer;
    [SyncVar]
    public NetworkInstanceId formId;

    void Awake() {
    }
}
