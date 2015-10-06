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

	public Block this[int x, int y] {
        get {
            var i = width * y + x;
            return blockArray[i];
        }

        set {
            var i = width * y + x;
            blockArray[i] = value;
        }
    }
}
