using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactionRelationEvent {
	Faction changedFaction;
	Faction otherFaction;
	int modifier;
}


public class Faction {
	private static Dictionary<string, Faction> byName = new Dictionary<string, Faction>();

	public static Faction Get(string name) {
		return byName[name];
	}

	public static void Setup() {
		Faction.Create("Dragons");
		Faction.Create("Mushrooms");
		Faction.Create("Bees");
	}

	public static void Create(string name) {
		var faction = new Faction();
		faction.name = name;
	}
	
	public string name;
}