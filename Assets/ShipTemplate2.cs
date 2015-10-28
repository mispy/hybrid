using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipTemplate2 : PoolBehaviour {
    static SensibleDictionary<string, ShipTemplate2> byId;

    public BlockMap blocks {
        get {
            return GetComponent<BlockMap>();
        }
    }

    static void LoadTemplates() {
        byId = new SensibleDictionary<string, ShipTemplate2>();
        foreach (var template in Game.LoadPrefabs<ShipTemplate2>("Ships")) {
            byId[template.name] = template;
        }
    }

    public static ShipTemplate2 FromId(string id) {
        if (byId == null) LoadTemplates();
        return byId[id];
    }

    public static ShipTemplate2[] All {
        get {
            if (byId == null) LoadTemplates();
            return byId.Values.ToArray();
        }
    }


    public static ShipTemplate2 FromShip(Ship ship) {
        var template = Pool.For("ShipTemplate").Attach<ShipTemplate2>(Game.state.transform);
        template.Fill(ship);
        return template;
    }
    
    public void Fill(Ship ship) {
        foreach (var block in ship.blueprintBlocks.allBlocks) {
            blocks[block.pos, block.layer] = new Block(block);
        }
    }

    public void Realize(Vector2 position) {
        var ship = Ship.FromTemplate(this);
        var form = ship.LoadBlockform();
        form.transform.position = position;
    }
}