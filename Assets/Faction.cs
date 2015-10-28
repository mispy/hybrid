﻿using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;

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

public class Faction : MonoBehaviour, IOpinionable {
    public static Dictionary<string, Faction> byId = new Dictionary<string, Faction>();
    public static Faction[] all {
        get { return Game.galaxy.GetComponentsInChildren<Faction>(); }
    }

    public static Faction Create(string name, Color? color = null) {
        if (color == null) color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        var faction = Pool.For("Faction").Attach<Faction>(Game.galaxy.factionHolder);
        faction.Initialize(name, (Color)color);
        return faction;
        
    }

    public static Faction FromId(string id) {
        if (byId.Keys.Count == 0) {
            foreach (var faction in Game.galaxy.GetComponentsInChildren<Faction>()) {
                byId[faction.id] = faction;
            }
        }
        return byId[id];
    }

    public FactionOpinion opinion;
    public Color color;

    public Faction() {
        this.opinion = new FactionOpinion(this);

    }

    public void Initialize(string name, Color color) {
        this.name = name;
        this.color = color;
    }    

    public string nameWithColor {
        get {
            return String.Format("<color='#{0}'>{1}</color>", ColorUtility.ToHtmlStringRGB(color), name);
        }
    }

	public string id {
        get { return name; }
    }

    public string savePath {
        get { return Application.dataPath + "/Faction/" + id + ".xml"; }
    }
}