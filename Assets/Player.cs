using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {
    public CrewBody crew { get; private set; }

    [Command]
    public void CmdMaglockMove(IntVector2 pos) {
        crew.MaglockMove(pos);
    }

    public void Awake() {
        crew = GetComponent<CrewBody>();
    }

    public override void OnStartLocalPlayer() {
        Game.localPlayer = this;        
        gameObject.AddComponent<CrewControl>();
    }
}
