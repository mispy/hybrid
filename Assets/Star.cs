using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Star : MonoBehaviour {
    public List<Sector> sectors = new List<Sector>();

    public static Star[] all {
        get { return Game.galaxy.stars.GetComponentsInChildren<Star>(); }
    }

    public Vector2 galaxyPos {
        get { return transform.position; }
    }

    public Vector2 BeaconPosition() {
        var systemRadius = 5f;
        var dir = Util.RandomDirection();
        return dir*Random.Range(2f, systemRadius);
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
