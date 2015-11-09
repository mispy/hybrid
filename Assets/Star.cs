using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Star : NetworkBehaviour {
    [SyncVar]
    public int x;

	void Start () {
	    var net = GetComponent<NetworkIdentity>();
	}
	
	void Update () {
        if (NetworkServer.active) {
            x += 1;
        } else {
            //Debug.Log(x);
        }
	}
}
