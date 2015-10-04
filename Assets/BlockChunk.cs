using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class BlockChunk {
    public int width = 32;
    public int height = 32;

    public Block[] blockArray;

    public BlockChunk() {
        blockArray = new Block[width*height];
    }

    public IEnumerable<Vector3> GetVertices(Block block) {
        var lx = block.localX * Tile.worldSize;
        var ly = block.localY * Tile.worldSize;
        
        var m = block.PercentFilled;
        
        yield return new Vector3(lx - Tile.worldSize*m / 2, ly + Tile.worldSize*m / 2, 0);
        yield return new Vector3(lx + Tile.worldSize*m / 2, ly + Tile.worldSize*m / 2, 0);
        yield return new Vector3(lx + Tile.worldSize*m / 2, ly - Tile.worldSize*m / 2, 0);
        yield return new Vector3(lx - Tile.worldSize*m / 2, ly - Tile.worldSize*m / 2, 0);
    }

    public IEnumerable<Block> AllBlocks {
        get {
            for (var i = 0; i < width; i++) {
                for (var j = 0; j < height; j++) {
                    var block = blockArray[width * j + i];
                    if (block != null) yield return block;
                }
            }
        }
    }

    public Block this[int x, int y] {
        get {
            var i = width * y + x;
            return blockArray[i];
        }

        set {
            Profiler.BeginSample("BlockChunk[x,y]");

            var i = width * y + x;
            blockArray[i] = value;


            Profiler.EndSample();
        }
    }
}
