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
		
	public static int wallLayer;
	public static int floorLayer;

	public static GameObject wallColliderPrefab;
	public static GameObject floorColliderPrefab;

	public static List<Rect> atlasBoxes = new List<Rect>();

	public static void Setup() {		
		Block.wallLayer = LayerMask.NameToLayer("Block");
		Block.floorLayer = LayerMask.NameToLayer("Floor");
		Block.wallColliderPrefab = Game.main.wallColliderPrefab;
		Block.floorColliderPrefab = Game.main.floorColliderPrefab;
	}
		
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
				var block = ship.blocks[ship.WorldToBlockPos(hit.transform.position)];
				if (block != null) {
					nearbyBlocks.Add(block);
				}
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

	public int collisionLayer;

	public Block(Ship ship, int type) {
		this.ship = ship;
		this.type = type;

		if (type == Block.types["floor"] || type == Block.types["console"]) {
			collisionLayer = LayerMask.NameToLayer("Floor");
		} else {
			collisionLayer = LayerMask.NameToLayer("Block");
		}
	}
}
