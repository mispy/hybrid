using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkSpawn : PoolBehaviour {    
    public void Start() {
        if (SpaceNetwork.isServer && !SpaceNetwork.spawnables.Contains(this.gameObject)) {
            SpaceNetwork.Spawn(this.gameObject);
        }
    }
}
