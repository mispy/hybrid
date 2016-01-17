using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Generate : MonoBehaviour {
    // Generate a random contiguous fragment of a Blockform
    public static Blockform Fragment(Vector2 pos, Blockform srcShip, int size) {
        var frag = new Blockform();
        frag.name = String.Format("Fragment ({0})", srcShip.name);

		var startBlock = srcShip.blocks[Util.GetRandom(srcShip.blocks.FilledPositions.ToList()), BlockLayer.Base];

        frag.blocks[startBlock.pos, BlockLayer.Base] = new Block(startBlock);
        var edges = new List<IntVector2>();
        edges.Add(startBlock.pos);

        while (frag.blocks.baseSize < size) {
            var edge = Util.GetRandom(edges);
            edges.Remove(edge);

            foreach (var neighborPos in IntVector2.Neighbors(edge)) {
                if (frag.blocks[neighborPos, BlockLayer.Base] == null) {
                    var srcBlock = srcShip.blocks[neighborPos, BlockLayer.Base];
                    if (srcBlock != null) {
                        frag.blocks[neighborPos, BlockLayer.Base] = new Block(srcBlock);
                        edges.Add(neighborPos);
                    }
                }
            }
        }
        return frag;
    }

    public static Blockform Asteroid(Vector2 pos, int radius) {
        var ship = Pool.For("Blockform").Attach<Blockform>(Game.activeSector.contents);
        ship.name = "Asteroid";

        for (var x = -radius; x < radius; x++) {
            for (var y = -radius; y < radius; y++) {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) <= radius) {
                    ship.blocks[x, y, BlockLayer.Base] = new Block(BlockType.FromId("Wall"));
                }
            }
        }

        ship.transform.position = pos;
        return ship;
    }
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}
