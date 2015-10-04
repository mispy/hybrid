using UnityEngine;
using System.Collections;

public class JumpShip : PoolBehaviour {
    public TileRenderer tiles;
    public Ship ship;
    float speed = 3f;

    public void Initialize(Ship ship) {
        this.ship = ship;
        ship.jumpShip = this;
        tiles.SetBlocks(ship.blocks);
        Rescale();
        SyncShip();
    }

    public override void OnCreate() {
        tiles = GetComponent<TileRenderer>();
    }

    public void SyncShip() {
        if (ship.sector == null) {
            transform.position = Game.jumpMap.GalaxyToWorldPos(ship.galaxyPos);
            if (ship.destSector != null)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, ship.destSector.galaxyPos - ship.galaxyPos);
        } else {
            ship.sector.jumpBeacon.Align(this);
        }
    }

    public void Rescale() {
        var desiredSize = 0.5f;
        var sizeX = Tile.worldSize * (ship.blocks.maxX - ship.blocks.minX);
        var sizeY = Tile.worldSize * (ship.blocks.maxY - ship.blocks.minY);
        transform.localScale = new Vector3(desiredSize/sizeX * 0.5f, desiredSize/sizeY * 0.5f, 1.0f);
    }
}
