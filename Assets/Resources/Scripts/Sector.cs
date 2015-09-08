using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
		get { return String.Format("{0}, {1}", cosmicX, cosmicY); }
	}

	public int cosmicX;
	public int cosmicY;
}
