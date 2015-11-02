using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpaceNetwork : NetworkManager {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnStartServer() {
        Game.Start();
    }
}
