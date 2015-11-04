using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpaceNetwork : NetworkManager {
    bool needsStart = false;

    public void Update() {
        if (needsStart) {
            Game.Start();
            needsStart = false;
        }
    }

    public override void OnStartServer() {
        needsStart = true;
    }
}
