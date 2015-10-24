using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Star {
    public GalaxyPos galaxyPos { get; private set; }
    public static List<Star> all = new List<Star>();
    public HashSet<Sector> sectors = new HashSet<Sector>();

    public static Star Create() {
        var galaxyPos = Game.galaxy.RandomPosition();
        return new Star(galaxyPos);
    }

    public GalaxyPos BeaconPosition() {
        var systemRadius = 5f;
        var dir = Util.RandomDirection();
        return new GalaxyPos(this, galaxyPos.vec + dir*Random.Range(2f, systemRadius));
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

    public Star(GalaxyPos pos) {
        this.galaxyPos = pos;
        Star.all.Add(this);
    }
}

public class Galaxy {
    public static float deltaTime;

    public void Simulate(float deltaTime) {
        Galaxy.deltaTime = deltaTime;

        foreach (var ship in Ship.all) {
            ship.Simulate(deltaTime);
        }
    }

    public GalaxyPos RandomPosition() {
        var cosmicWidth = 100;
        var cosmicHeight = 100;    
        var x = Random.Range(-cosmicWidth/2, cosmicWidth/2);
        var y = Random.Range(-cosmicHeight/2, cosmicHeight/2);
        return new GalaxyPos(x, y);
    }
}
