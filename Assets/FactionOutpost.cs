using UnityEngine;
using System.Collections;

public class FactionOutpost : PoolBehaviour {
    public Ship station { get; private set; }
    public Sprite sprite {
        get { return Game.Sprite("TradeStation"); }
    }
    
    public static FactionOutpost Create(Star star, Faction faction = null, Ship station = null) {
        if (faction == null && station == null) faction = Util.GetRandom(Faction.all);
        
        
        var beacon = Pool.For("FactionOutpost").Attach<FactionOutpost>(star.transform);
        beacon.transform.localPosition = star.BeaconPosition();
        return beacon;
    }
    
    public string Describe() {
        var s = "";
        s += "<color='yellow'>Trade Station</color>\n";
        s += "Intensity: Low\n";
        return s;
    }
}
