using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {
    public CrewBody crew { get; private set; }

    public void Awake() {
        crew = GetComponent<CrewBody>();
    }

    public void Start() {
        Game.localPlayer = this;        
        Game.cameraControl.Lock(transform);
        gameObject.AddComponent<CrewControl>();
    }
}
