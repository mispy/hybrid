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
    // Scale the passage of time for the rest of the galaxy while
    // inside a sector
    public static float timeScale;
    public static float deltaTime;
    public Transform starHolder;
    public Transform factionHolder;
    public Transform shipHolder;
    public Transform crewHolder;

    public GalaxyPos RandomPosition() {
        var cosmicWidth = 100;
        var cosmicHeight = 100;    
        var x = Random.Range(-cosmicWidth/2, cosmicWidth/2);
        var y = Random.Range(-cosmicHeight/2, cosmicHeight/2);
        return new GalaxyPos(x, y);
    }

    public void Generate() {
        Util.DestroyChildrenImmediate(transform);

        starHolder = Pool.For("Holder").Attach<Transform>(transform);
        starHolder.name = "Stars";

        factionHolder = Pool.For("Holder").Attach<Transform>(transform);
        factionHolder.name = "Factions";

        shipHolder = Pool.For("Holder").Attach<Transform>(transform);
        shipHolder.name = "Ships";

        crewHolder = Pool.For("Holder").Attach<Transform>(transform);
        crewHolder.name = "Crew";

        for (var i = 0; i < 100; i++) {
            var star = Pool.For("Star").Attach<Star>(starHolder);
            star.transform.position = RandomPosition().vec;
        }

        Faction.Create("Dragons");
        Faction.Create("Mushrooms");
        var mitzubi = Faction.Create("Mitzubi Navy", color: new Color(251/255.0f, 213/255.0f, 18/255.0f));
        Faction.Create("Cats");
        var pirateGang = Faction.Create("Pirate Gang", color: Color.red);
        
        pirateGang.opinion[mitzubi].Change(-1000, OpinionReason.AttackedMyShip);
        mitzubi.opinion[pirateGang].Change(-1000, OpinionReason.AttackedMyShip);
        
        foreach (var star in Game.galaxy.GetComponentsInChildren<Star>()) {           
            FactionOutpost.Create(star, faction: mitzubi);
            //ConflictZone.Create(star.BeaconPosition(), attacking: pirateGang, defending: mitzubi);
        }
    }
}
