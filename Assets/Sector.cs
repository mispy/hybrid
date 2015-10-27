using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class SectorManager {
    public static List<Sector> all = new List<Sector>();
    public static Dictionary<string, Sector> byId = new Dictionary<string, Sector>();
    public static void Add(Sector sector) {
        SectorManager.all.Add(sector);
        SectorManager.byId[sector.Id] = sector;
    }
}

public interface ISectorType {
    void OnRealize();
    Sprite sprite { get; }
    string Describe();
}

public class FactionOutpost : ISectorType {
    public Sector sector;
    public Ship station { get; private set; }
    public Sprite sprite {
        get { return Game.Sprite("TradeStation"); }
    }

    public static FactionOutpost Create(GalaxyPos galaxyPos = null, Faction faction = null, Ship station = null) {
        if (galaxyPos == null) galaxyPos = Game.galaxy.RandomPosition();
        if (faction == null && station == null) faction = Util.GetRandom(Faction.all);


        if (station == null) 
            station = Ship.Create("Station", faction: faction);

        return new FactionOutpost((GalaxyPos)galaxyPos, station);
    }

    public FactionOutpost(GalaxyPos galaxyPos, Ship station) {
        this.station = station;
        var sector = new Sector(this, galaxyPos);
        SectorManager.Add(sector);
        sector.PlaceShip(station, Vector2.zero);
    }

    public void OnRealize() { }

    public string Describe() {
        var s = "";
        s += "<color='yellow'>Trade Station</color>\n";
        s += "Intensity: Low\n";
        return s;
    }
}

public class ConflictZone : ISectorType {
    public Sector sector;
    public Faction attacking;
    public Faction defending;
    public Sprite sprite {
        get { return Game.Sprite("ConflictZone"); }
    }

    public static ConflictZone Create(GalaxyPos galaxyPos = null, Faction attacking = null, Faction defending = null) {
        if (galaxyPos == null) galaxyPos = Game.galaxy.RandomPosition();
        if (attacking == null) attacking = Util.GetRandom(Faction.all);
        if (defending == null) defending = Util.GetRandom(Faction.all);

        return new ConflictZone(galaxyPos, attacking, defending);
    }

    public ConflictZone(GalaxyPos galaxyPos, Faction attacking, Faction defending) {
        this.sector = new Sector(this, galaxyPos);
        this.attacking = attacking;
        this.defending = defending;
    }

    public void OnRealize() {
        Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: defending);
        Ship.Create(sector: sector, faction: defending);
        Ship.Create(sector: sector, faction: defending);
    }

    public string Describe() {
        var s = "";
        s += "<color='red'>Conflict Zone</color>\n";
        s += "Intensity: Low\n";
//        s += "{0} is fighting {1} for control of this star."
        return s;
    }
}

[Serializable]
public class Sector {
    public ISectorType type;

    public string Id {
        get { return String.Format("{0}, {1}", galaxyPos.x, galaxyPos.y); }
    }

    public GalaxyPos galaxyPos;
    public float radius = 200f;

    public List<Ship> ships = new List<Ship>();
    public JumpBeacon jumpBeacon;
    
    public Vector2 RandomEdge() {
        return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))*radius*1.5f;
    }

    public void JumpEnterShip(Ship ship, Vector2 entryVector) {
        var pos = radius * 1.2f * (entryVector.normalized * -1f);
        PlaceShip(ship, pos);
    }

    public void PlaceShip(Ship ship, Vector2 pos) {
        ship.destSector = null;
        if (ship.sector != null)
            ship.sector.ships.Remove(ship);
        ship.sector = this;
        ship.galaxyPos = galaxyPos;
        ships.Add(ship);
        
        ship.sectorPos = pos;
        
        if (this == Game.activeSector.sector) {
            Game.activeSector.RealizeShip(ship);
        }
    }

    public Sector(ISectorType type, GalaxyPos galaxyPos) {
        this.type = type;
        this.galaxyPos = galaxyPos;
        if (galaxyPos.star != null)
            galaxyPos.star.sectors.Add(this);
        SectorManager.all.Add(this);
    }
}
