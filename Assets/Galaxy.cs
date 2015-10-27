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
    }

    void Awake() {
        FactionManager.Create("Dragons");
        FactionManager.Create("Mushrooms");
        var mitzubi = FactionManager.Create("Mitzubi Navy", color: new Color(251/255.0f, 213/255.0f, 18/255.0f));
        FactionManager.Create("Cats");
        var pirateGang = FactionManager.Create("Pirate Gang", color: Color.red);
        
        pirateGang.opinion[mitzubi].Change(-1000, OpinionReason.AttackedMyShip);
        mitzubi.opinion[pirateGang].Change(-1000, OpinionReason.AttackedMyShip);
        
        foreach (var star in Game.galaxy.stars.GetComponentsInChildren<Star>()) {
            FactionOutpost.Create(star.BeaconPosition(), faction: mitzubi);
            ConflictZone.Create(star.BeaconPosition(), attacking: pirateGang, defending: mitzubi);
        }
        
        
        var sector = SectorManager.all[0];
        //ShipManager.Create(sector: sector, faction: FactionManager.all[1], sectorPos: new Vector2(100, 0));
        Game.playerShip = Ship.Create(sector: sector, faction: mitzubi, sectorPos: new Vector2(-100, 0));
    }
}
