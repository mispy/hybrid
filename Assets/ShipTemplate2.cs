using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipTemplate2 : PoolBehaviour {
    static SensibleDictionary<string, ShipTemplate2> byId;

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

    public string tagged = "";

    public string[] tags {
        get {
            return tagged.Split(' ');
        }
    }

    public BlockMap blocks {
        get {
            return GetComponent<BlockMap>();
        }
    }
    
    public void Fill(Blockform ship) {
        foreach (var block in ship.blocks.allBlocks) {
            blocks[block.pos, block.layer] = new Block(block);
        }
    }
}