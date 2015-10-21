using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
    public Sector sector;
    public Transform contents;
    public Transform transients;

    public List<Blockform> blockforms;

    public bool IsOutsideBounds(Vector3 pos) {
        return pos.magnitude > sector.radius;
    }

    public void Awake() {
        contents = Pool.For("Holder").Attach<Transform>(transform);
        contents.name = "Contents";
        transients = Pool.For("Holder").Attach<Transform>(transform);
        transients.name = "Transients";
    }

    public void Update() {
        Game.galaxy.Simulate(Time.deltaTime);
    }

    public void RealizeShip(Ship ship, Vector2 pos) {
        var form = ship.LoadBlockform();
        form.transform.SetParent(contents);
		form.transform.position = pos;
        form.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector2.zero - pos);
        form.gameObject.SetActive(true);
    }

    public void RealizeShip(Ship ship) {
        RealizeShip(ship, ship.sectorPos);
    }
}
