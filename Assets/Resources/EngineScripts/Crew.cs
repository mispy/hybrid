using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CrewManager {
    public static List<Crew> all = new List<Crew>();
    public static Dictionary<string, Crew> byId = new Dictionary<string, Crew>();
    
    public static void LoadAll() {
        foreach (var path in Save.GetFiles("Crew")) {
            var crew = Save.Load<Crew>(path);
            CrewManager.Add(crew);
        }
    }
    
    public static void SaveAll() {
        foreach (var crew in CrewManager.all) {
            Save.Dump(crew, Save.GetPath("Crew", crew.Id));
        }
    }
    
    public static void Add(Crew crew) {
        CrewManager.all.Add(crew);
        CrewManager.byId[crew.Id] = crew;
    }
}

[Serializable]
public class Crew {

    private Ship ship;
    public Ship Ship {
        get { return ship; }
        set {
            if (ship != null) {
                ship.crew.Remove(this);
            }
            ship = value;
            ship.crew.Add(this);
        }
    }

    public int maxHealth = 100;
    public int health = 100;
    public string name;
    public Faction faction;

    public string Id {
        get { return name; }
    }

    public Crew(string name) {
        this.name = name;
    }
}