using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkSpawn : PoolBehaviour {    
    public void Start() {
        if (NetworkServer.active && guid.value == null) {
            SpaceNetwork.Spawn(this.gameObject);
        }
    }
}
