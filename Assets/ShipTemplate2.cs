using UnityEngine;
using System.Collections;

public class ShipTemplate2 : PoolBehaviour {
    BlockMap blocks;
    
    public static ShipTemplate2 FromShip(Ship ship) {
        var template = Pool.For("ShipTemplate").Attach<ShipTemplate2>(Game.state.transform);
        template.Fill(ship);
        return template;
    }
    
    public void Fill(Ship ship) {
        blocks = GetComponent<BlockMap>();
        foreach (var block in ship.blueprintBlocks.allBlocks) {
            blocks[block.pos, block.layer] = new Block(block);
        }
    }
}