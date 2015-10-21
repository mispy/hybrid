using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IShipMission {
    void Simulate();
}

public class PatrolMission : IShipMission {
    public Ship ship;
    public List<Star> stars;

    public bool isScanning = false;
    public float scanDuration = 0f;

    public PatrolMission(Ship ship, List<Star> stars) {
        this.ship = ship;
        this.stars = stars;
    }

    public Sector ChooseDestination() {
        var star = Util.GetRandom(stars);
        var sector = Util.GetRandom(star.sectors);
        return sector;
    }

    public void Simulate() {        
        if (!isScanning && !ship.inTransit) {
            // We've just reached a destination, start scanning
            isScanning = true;
            scanDuration = 5f;
        }

        if (isScanning) {
            scanDuration -= Galaxy.deltaTime;

            if (scanDuration <= 0f) {
                // Scan complete, let's go somewhere else
                isScanning = false;
                var sector = ChooseDestination();
                ship.FoldJump(sector);
            }
        }
    }
}

public class ShipStrategy {
    public Ship ship;
    public IShipMission mission;

    public ShipStrategy(Ship ship) {
        this.ship = ship;
    }

    public void Simulate() {
        if (ship.isStationary) return;

        if (mission == null) {
            var starsToPatrol = new List<Star>();
            foreach (var star in Star.all) {
                if (star.faction == ship.faction)
                    starsToPatrol.Add(star);
            }

            if (starsToPatrol.Count > 0)
                mission = new PatrolMission(ship, starsToPatrol);
        }

        if (mission != null)
            mission.Simulate();
    }
}
