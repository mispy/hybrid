using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {
    [Command]
    public void CmdGetWorld() {
        foreach (var form in Game.activeSector.blockforms) {
            form.Propagate();
        }
    }

    public override void OnStartLocalPlayer() {
        CmdGetWorld();
    }
}
