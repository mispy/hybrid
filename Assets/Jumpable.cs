using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jumpable : PoolBehaviour {
    [HideInInspector]
    public new SpriteRenderer renderer;
    public Sector sector;

    public Vector2 galaxyPos {
        get { return (Vector2)transform.position; }
    }

    void Awake() {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Align(JumpShip jumpShip) {
        var size = renderer.bounds.size;
        jumpShip.transform.position = new Vector2(transform.position.x + size.x, transform.position.y + size.y + 0.3f*sector.ships.IndexOf(jumpShip.ship));
    }
}
