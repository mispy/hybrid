using UnityEngine;
using System.Collections;

public class SectorKind : MonoBehaviour {
    public Marker[] markers {
        get {
            return GetComponentsInChildren<Marker>();
        }
    }

    public float radius {
        get {
            return transform.Find("Bounds").transform.localScale.x;
        }
    }

    public void OnEnable() {
        foreach (var marker in markers) {
            marker.Realize();
        }

        Game.activeSector.RealizeShip(Game.playerShip, new Vector2(0, radius));
        Game.state.gameObject.SetActive(true);
        Game.activeSector.gameObject.SetActive(true);
    }
}
