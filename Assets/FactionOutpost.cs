using UnityEngine;
using System.Collections;

public class FactionOutpost : MonoBehaviour {
    public Sector sector;
    public Ship station { get; private set; }
    public Sprite sprite {
        get { return Game.Sprite("TradeStation"); }
    }
    
    public static FactionOutpost Create(Star star, Faction faction = null, Ship station = null) {
        if (faction == null && station == null) faction = Util.GetRandom(Faction.all);
        
        
        var beacon = Beacon.Create(star);
        beacon.GetComponent<SpriteRenderer>().sprite = Game.Sprite("TradeStation");
        return beacon.gameObject.AddComponent<FactionOutpost>();
    }
    
    public void OnRealize() { }
    
    public string Describe() {
        var s = "";
        s += "<color='yellow'>Trade Station</color>\n";
        s += "Intensity: Low\n";
        return s;
    }
}
