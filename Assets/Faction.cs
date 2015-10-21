using UnityEngine;
using Random = UnityEngine.Random;
using System;
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
		if (color == null) color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
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

public class FactionOpinion {
    public readonly Faction faction;
    public readonly Dictionary<IOpinionable, OpinionOf> opinions = new Dictionary<IOpinionable, OpinionOf>();

    public FactionOpinion(Faction faction) {
        this.faction = faction;
    }
    
    public OpinionOf this[Ship ship] {
        get {
            if (!opinions.ContainsKey(ship)) {
                opinions[ship] = new OpinionOf(ship);
                opinions[ship].Change(this[ship.faction].amount, OpinionReason.FactionOpinion);
            }

            return opinions[ship];
        }
    }

    public OpinionOf this[Faction faction] {
        get {
            if (!opinions.ContainsKey(faction)) {
                opinions[faction] = new OpinionOf(faction);
                if (faction == this.faction)
                    opinions[faction].Change(100, OpinionReason.SameFaction);
            }

            return opinions[faction];
        }
    }
}

public class Faction : IOpinionable {
    public string name;
    public FactionOpinion opinion;
    public Color color;

    public Faction(string name, Color color) {
        this.name = name;
        this.color = color;
        this.opinion = new FactionOpinion(this);
    }    

    public string nameWithColor {
        get {
            return String.Format("<color='#{0}'>{1}</color>", ColorUtility.ToHtmlStringRGB(color), name);
        }
    }

	public string Id {
        get { return name; }
    }
}