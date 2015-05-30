using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block {
	public static Dictionary<string, int> types = new Dictionary<string, int>();

	public static int pixelSize = 32; // the size of a block in pixels
	public static float worldSize = 1f; // the size of the block in worldspace coordinates

	// size of a block as fraction of tilesheet size
	public static float tileWidth = 0; 
	public static float tileHeight = 0;

	public static string GetTypeName(int blockType) {
		foreach (var key in types.Keys) {
			if (types[key] == blockType)
				return key;
		}

		throw new KeyNotFoundException();
	}

	public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics2D.OverlapCircleAll(center, radius);

		List<Block> nearbyBlocks = new List<Block>();

		foreach (var hit in hits) {
			if (hit.gameObject.transform.parent != null) {
				var ship = hit.gameObject.transform.parent.GetComponent<Ship>();
				nearbyBlocks.Add(ship.blocks[ship.WorldToBlockPos(hit.transform.position)]);
			}
		}

		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.ship.BlockToWorldPos(block.pos)));
	}

	/*public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics2D.OverlapCircleAll(center, radius);
		
		List<Block> nearbyBlocks = new List<Block>();
		
		foreach (var hit in hits) {
			if (hit.gameObject.transform.parent != null) {
				nearbyBlocks.Add(hit.gameObject.GetComponent<Block>());
				//Camera.main.transform.parent = activeShip.gameObject.transform;
			}
		}
		
		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.transform.position));
	}*/

	public Ship ship;
	public IntVector2 pos = new IntVector2();
	public int type;
	public Vector2 orientation = -Vector2.right;
	
	public bool touched = true;

	public Block(Ship ship, int type) {
		this.ship = ship;
		this.type = type;
	}
}
