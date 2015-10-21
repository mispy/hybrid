using UnityEngine;
using System.Collections;

public class JumpShip : PoolBehaviour {
    public Ship ship;
    float speed = 3f;
    public SpriteRenderer spriteRenderer;

    public void Initialize(Ship ship) {
        this.ship = ship;
        ship.jumpShip = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = ship.faction.color;
        SyncShip();
    }

    public override void OnCreate() {
    }

    public void SyncShip() {
        if (ship.sector == null) {
            transform.position = Game.jumpMap.GalaxyToWorldPos(ship.galaxyPos);
            if (ship.destSector != null)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, (Vector2)ship.destSector.galaxyPos - (Vector2)ship.galaxyPos);
        } else {
            ship.sector.jumpBeacon.Align(this);
        }
    }
}
