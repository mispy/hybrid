using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Generate : MonoBehaviour {
    // Generate a random contiguous fragment of a ship
    public static Ship Fragment(Vector2 pos, Ship srcShip, int size) {
        var frag = new Ship();
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

    public static Ship Asteroid(Vector2 pos, int radius) {
        var shipObj = Pool.For("Ship").TakeObject();
        var ship = shipObj.GetComponent<Ship>();

        ship.name = "Asteroid";
        for (var x = -radius; x < radius; x++) {
            for (var y = -radius; y < radius; y++) {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) <= radius) {

                    //var ori = new Vector2[] { Vector2.up, Vector2.right, -Vector2.up, -Vector2.right };
                    ship.SetBlock(x, y, Block.typeByName["Wall"]);
                    //ship.SetBlock(x, y, "wall"], ori[Random.Range(0, 3)]);
                }
            }
        }
        shipObj.transform.position = pos;
        shipObj.SetActive(true);
        return ship;
    }
    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}
