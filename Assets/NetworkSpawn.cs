using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkSpawn : PoolBehaviour {    
    public void Start() {
        if (SpaceNetwork.isServer && guid.value == null) {
            SpaceNetwork.Spawn(this.gameObject);
        }
    }
}
