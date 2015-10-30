using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ConflictZone : MonoBehaviour {
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
        this.attacking = attacking;
        this.defending = defending;
    }

    public void OnRealize() {
        /*Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: attacking);
        Ship.Create(sector: sector, faction: defending);
        Ship.Create(sector: sector, faction: defending);
        Ship.Create(sector: sector, faction: defending);*/
    }

    public string Describe() {
        var s = "";
        s += "<color='red'>Conflict Zone</color>\n";
        s += "Intensity: Low\n";
//        s += "{0} is fighting {1} for control of this star."
        return s;
    }
}