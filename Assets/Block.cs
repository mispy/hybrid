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

	public static float mass = 0.0001f;

	// the core sequence of each block type sprite
	public static Texture2D[] sprites;

	// precalculated uv coordinates for each block type and orientation
	public static Vector2[][] upUVs;
	public static Vector2[][] downUVs;
	public static Vector2[][] leftUVs;
	public static Vector2[][] rightUVs;

	public static void Setup(Texture2D[] blockSprites) {
		Block.sprites = blockSprites;
		Block.wallLayer = LayerMask.NameToLayer("Block");
		Block.floorLayer = LayerMask.NameToLayer("Floor");
		Block.wallColliderPrefab = Game.main.wallColliderPrefab;
		Block.floorColliderPrefab = Game.main.floorColliderPrefab;
				
		// let's compress all the block sprites into a single tilesheet texture
		var atlas = new Texture2D(Block.pixelSize*100, Block.pixelSize*100);

		var boxes = atlas.PackTextures(Block.sprites, 0, Block.pixelSize*Block.sprites.Count());

		Block.tileWidth = (float)Block.pixelSize / atlas.width;
		Block.tileHeight = (float)Block.pixelSize / atlas.height;

		upUVs = new Vector2[Block.sprites.Length][];
		downUVs = new Vector2[Block.sprites.Length][];
		leftUVs = new Vector2[Block.sprites.Length][];
		rightUVs = new Vector2[Block.sprites.Length][];

		for (var i = 0; i < Block.sprites.Length; i++) {			
			Block.types[Block.sprites[i].name] = i;

			var box = boxes[i];

			upUVs[i] = new Vector2[] {
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin, box.yMin)
			};

			downUVs[i] = new Vector2[] {
				new Vector2(box.xMin, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin + Block.tileHeight)
			};

			leftUVs[i] = new Vector2[] {
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin)
			};

			rightUVs[i] = new Vector2[] {
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight)
			};
		}

		Game.main.shipPrefab.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
	}
		
	public static string GetTypeName(int blockType) {
		foreach (var key in types.Keys) {
			if (types[key] == blockType)
				return key;
		}

		throw new KeyNotFoundException();
	}

	public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics.OverlapSphere(center, radius);

		List<Block> nearbyBlocks = new List<Block>();

		foreach (var hit in hits) {
			if (hit.gameObject.transform.parent != null) {
				var ship = hit.gameObject.transform.parent.GetComponent<Ship>();
				if (ship != null) {
					var block = ship.blocks[ship.WorldToBlockPos(hit.transform.position)];
					if (block != null)
						nearbyBlocks.Add(block);
				}
			}
		}

		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.ship.BlockToWorldPos(block.pos)));
	}

	public static IEnumerable<Block> FromHits(RaycastHit[] hits) {
		foreach (var hit in hits) {
			var ship = hit.rigidbody.gameObject.GetComponent<Ship>();
			if (ship != null) {
				var block = ship.BlockAtWorldPos(hit.collider.transform.position);
				//Debug.LogFormat("{0} {1}", ship.WorldToBlockPos(hit.collider.transform.position), block);
				if (block != null)
					yield return block;
			}
		}
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

	public int index;
		
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
