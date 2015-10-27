using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Star : MonoBehaviour {
    public List<Sector> sectors = new List<Sector>();

    public Vector2 galaxyPos {
        get { return transform.position; }
    }

    public GalaxyPos BeaconPosition() {
        var systemRadius = 5f;
        var dir = Util.RandomDirection();
        return new GalaxyPos(this, galaxyPos + dir*Random.Range(2f, systemRadius));
    }
    
    public Faction faction {
        get {
            foreach (var sector in sectors) {
                var outpost = sector.type as FactionOutpost;
                if (outpost != null)
                    return outpost.station.faction;
            }
            
            return null;
        }
    }
}
