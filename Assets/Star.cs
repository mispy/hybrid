using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Star : MonoBehaviour {
    public static Star[] all {
        get { return Game.state.GetComponentsInChildren<Star>(); }
    }

    public Vector2 galaxyPos {
        get { return transform.position; }
    }

    public Vector2 BeaconPosition() {
        var systemRadius = 2f;
        var dir = Util.RandomDirection();
        return dir*Random.Range(1f, systemRadius);
    }
    
    public Faction faction {
        get {
            /*foreach (var sector in sectors) {
                var outpost = sector.type as FactionOutpost;
                if (outpost != null)
                    return outpost.station.faction;
            }*/
            
            return null;
        }
    }
}
