using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
    public Sector sector;

    public List<Blockform> blockforms;

    public GameObject leaveSectorMenu;

    public bool IsOutsideBounds(Vector3 pos) {
        return pos.magnitude > sector.radius;
    }

    void Update() {
        if (IsOutsideBounds(Game.playerShip.form.transform.position)) {
            leaveSectorMenu.SetActive(true);
        } else {
            leaveSectorMenu.SetActive(false);
        }

        Game.galaxy.Simulate(Time.deltaTime);

        
        Game.MoveCamera(Game.playerShip.form.transform.position);
        Game.mainCamera.transform.rotation = Game.playerShip.form.transform.rotation;
    }

    public void RealizeShip(Ship ship, Vector2 pos) {
        var form = ship.LoadBlockform();
        form.transform.parent = transform;
        form.transform.position = pos;
        form.gameObject.SetActive(true);
    }

    public void RealizeShip(Ship ship) {
        RealizeShip(ship, ship.sectorPos);
    }
}
