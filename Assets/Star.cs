using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Star : NetworkBehaviour {
    public static void Setup() {
        ClientScene.RegisterSpawnHandler(Game.Prefab("Star").GetComponent<NetworkIdentity>().assetId, OnSpawn, OnDeSpawn);
    }

    public static GameObject OnSpawn(Vector3 position, NetworkHash128 assetId) {
        Debug.Log("OnSpawn");
        var star = Pool.For("Star").Attach<Star>(Game.state.transform, false);
        star.transform.position = position;
        star.gameObject.SetActive(false);
        return star.gameObject;
    }

    public static void OnDeSpawn(GameObject obj) {
        Destroy(obj);
    }

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
