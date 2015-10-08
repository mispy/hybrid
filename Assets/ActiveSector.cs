using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
    public Sector sector;

    public List<Blockform> blockforms;

    public bool IsOutsideBounds(Vector3 pos) {
        return pos.magnitude > sector.radius;
    }

    void Update() {
        Game.galaxy.Simulate(Time.deltaTime);
    }

    public void RealizeShip(Ship ship, Vector2 pos) {
        var form = ship.LoadBlockform();
        form.transform.parent = transform;
		form.transform.position = pos;
        form.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector2.zero - pos);
        form.gameObject.SetActive(true);
    }

    public void RealizeShip(Ship ship) {
        RealizeShip(ship, ship.sectorPos);
    }
}
