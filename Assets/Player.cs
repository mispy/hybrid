using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {
    public CrewBody crew { get; private set; }

    [Command]
    public void CmdGetWorld() {
        foreach (var form in Game.activeSector.blockforms) {
            form.Propagate();
        }
    }

    [Command]
    public void CmdMaglockMove(IntVector2 pos) {
        crew.MaglockMove(pos);
    }

    public void MaglockMove(IntVector2 pos) {
        CmdMaglockMove(pos);
    }

    public override void OnStartServer() {
        crew = GetComponent<CrewBody>();
    }

    public override void OnStartLocalPlayer() {
        CmdGetWorld();
    }
}
