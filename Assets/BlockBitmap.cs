using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockBitmap : IEnumerable<IntVector2> {
	BlockMap blocks;
	bool[,] bitmap;
	public int size = 0;

	public BlockBitmap(BlockMap blocks) {
		this.blocks = blocks;
		bitmap = new bool[blocks.width+2, blocks.height+2];
		for (var i = 0; i < blocks.width+2; i++) {
			for (var j = 0; j < blocks.height+2; j++) {
				bitmap[i, j] = false;
			}
		}
	}

	public bool this[IntVector2 bp] {
		get {
			return bitmap[bp.x-blocks.minX+1, bp.y-blocks.minY+1];
		}

		set {
			if (value == true && this[bp] == false) size += 1;
			if (value == false && this[bp] == true) size -= 1;
			bitmap[bp.x-blocks.minX+1, bp.y-blocks.minY+1] = value;
		}
	}

	public IEnumerator<IntVector2> GetEnumerator()
	{
		for (var i = blocks.minX-1; i <= blocks.maxX+1; i++) {
			for (var j = blocks.minY-1; j <= blocks.maxY+1; j++) {
				var pos = new IntVector2(i, j);
				if (this[pos] == true)
					yield return pos;
			}
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}