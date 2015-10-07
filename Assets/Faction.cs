using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class FactionManager {
    public static List<Faction> all = new List<Faction>();
    public static Dictionary<string, Faction> byId = new Dictionary<string, Faction>();
    
    public static void LoadAll() {
        foreach (var path in Save.GetFiles("Faction")) {
            var faction = Save.Load<Faction>(path);
            FactionManager.Add(faction);
        }
    }
    
    public static void SaveAll() {
        foreach (var faction in FactionManager.all) {
            Save.Dump(faction, Save.GetPath("Faction", faction.Id));
        }
    }
    
    public static void Add(Faction faction) {
        FactionManager.all.Add(faction);
        FactionManager.byId[faction.Id] = faction;
    }

	public static Faction Create(string name, Color? color = null) {
		if (color == null) color = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
		var faction = new Faction(name, (Color)color);
		FactionManager.Add(faction);
		return faction;
	}
}

public class FactionRelationEvent {
    Faction changedFaction;
    Faction otherFaction;
    int modifier;
}

public class Faction {
    public string name;
	public Color color;

	public string Id {
        get { return name; }
    }

    public Faction(string name, Color color) {
        this.name = name;
		this.color = color;
    }

    public bool IsEnemy(Faction other) {
        return other != this && other != FactionManager.all[0] && this != FactionManager.all[0];
    }
}