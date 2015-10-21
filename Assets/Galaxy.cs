using UnityEngine;
using System.Collections;

public class Galaxy {
    public void Simulate(float deltaTime) {
        foreach (var ship in ShipManager.all) {
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
}
