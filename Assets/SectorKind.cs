using UnityEngine;
using System.Collections;

public class SectorKind : MonoBehaviour {
    public Marker[] markers {
        get {
            return GetComponentsInChildren<Marker>();
        }
    }

    public SectorBounds bounds {
        get {
            return GetComponentInChildren<SectorBounds>();
        }
    }

    public void OnEnable() {
        Game.activeSector.RealizeShip(Game.playerShip, new Vector2(0, -bounds.transform.localScale.y));
        Game.state.gameObject.SetActive(true);
        Game.activeSector.gameObject.SetActive(true);
    }
}
