using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Galaxy))]
public class GalaxyEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Galaxy galaxy = (Galaxy)target;

        if (GUILayout.Button("Generate")) {
            galaxy.Generate();
        }
    }
}

public class Galaxy : PoolBehaviour {
    public static float deltaTime;
    public Transform stars;
    public Transform factionHolder;

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

    public void Generate() {
        Util.DestroyChildrenImmediate(transform);

        stars = Pool.For("Holder").Attach<Transform>(transform);
        stars.name = "Stars";

        for (var i = 0; i < 100; i++) {
            var star = Pool.For("Star").Attach<Star>(stars);
            star.transform.position = RandomPosition().vec;
        }


        factionHolder = Pool.For("Holder").Attach<Transform>(transform);
        factionHolder.name = "Factions";

        Faction.Create("Dragons");
        Faction.Create("Mushrooms");
        var mitzubi = Faction.Create("Mitzubi Navy", color: new Color(251/255.0f, 213/255.0f, 18/255.0f));
        Faction.Create("Cats");
        var pirateGang = Faction.Create("Pirate Gang", color: Color.red);
        
        pirateGang.opinion[mitzubi].Change(-1000, OpinionReason.AttackedMyShip);
        mitzubi.opinion[pirateGang].Change(-1000, OpinionReason.AttackedMyShip);
        
        foreach (var star in Game.galaxy.stars.GetComponentsInChildren<Star>()) {           
            FactionOutpost.Create(star, faction: mitzubi);
            //ConflictZone.Create(star.BeaconPosition(), attacking: pirateGang, defending: mitzubi);
        }
    }

    void Awake() {
        var beacon = Util.GetRandom(Star.all).GetComponentInChildren<Beacon>();
        //ShipManager.Create(sector: sector, faction: FactionManager.all[1], sectorPos: new Vector2(100, 0));
        Game.playerShip = Ship.Create(beacon: beacon, faction: Faction.FromId("Mitzubi Navy"));
    }
}
