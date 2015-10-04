using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class SectorManager {
    public static List<Sector> all = new List<Sector>();
    public static Dictionary<string, Sector> byId = new Dictionary<string, Sector>();

    public static void LoadAll() {
        foreach (var path in Save.GetFiles("Sector")) {
            var sector = Save.Load<Sector>(path);
            SectorManager.Add(sector);
        }
    }

    public static void SaveAll() {
        foreach (var sector in SectorManager.all) {
            Save.Dump(sector, Save.GetPath("Sector", sector.Id));
        }
    }

    public static void Add(Sector sector) {
        SectorManager.all.Add(sector);
        SectorManager.byId[sector.Id] = sector;
    }
}

[Serializable]
public class Sector {
    public string Id {
        get { return String.Format("{0}, {1}", galaxyPos.x, galaxyPos.y); }
    }

    public Vector2 galaxyPos;
    public float radius = 200f;

    public List<Ship> ships = new List<Ship>();
    public JumpBeacon jumpBeacon;
    
    public Vector2 RandomEdge() {
        return new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))*radius*1.5f;
    }

    public void PlaceShip(Ship ship, Vector2 entryVector) {
        ship.destSector = null;
        if (ship.sector != null)
            ship.sector.ships.Remove(ship);
        ship.sector = this;
        ship.galaxyPos = galaxyPos;
        ships.Add(ship);

        ship.sectorPos = radius * 1.2f * (entryVector.normalized * -1f);

        if (this == Game.activeSector.sector) {
            Game.activeSector.RealizeShip(ship);
        }
    }
}
