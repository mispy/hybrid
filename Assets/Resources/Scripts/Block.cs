using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block {
	public static Dictionary<string, BlockType> types = new Dictionary<string, BlockType>();
	public static List<BlockType> allTypes = new List<BlockType>();

	public static int pixelSize = 32; // the size of a block in pixels
	public static float worldSize = 1f; // the size of the block in worldspace coordinates

	// size of a block as fraction of tilesheet size
	public static float tileWidth = 0; 
	public static float tileHeight = 0;
		
	public static int wallLayer;
	public static int floorLayer;

	public static GameObject wallColliderPrefab;
	public static GameObject floorColliderPrefab;

	public static string[] blockOrder = new string[] {
		"floor",
		"wall",
		"console",
		"thruster",
		"tractorBeam",
		"gravgen",
		"shieldgen",
		"torpedoLauncher"
	};

	public static void Setup() {
		foreach (var type in Game.LoadPrefabs<BlockType>("Blocks")) {
			type.name = type.gameObject.name;
			Block.types[type.name] = type;
		}

		foreach (var name in blockOrder) {
			Block.allTypes.Add(Block.types[name]);
		}

		foreach (var type in Block.types.Values) {
			if (!Block.allTypes.Contains(type))
				Block.allTypes.Add(type);
		}
		Texture2D[] blockTextures = Block.allTypes.Select(type => type.texture).ToArray();
		
		Block.wallLayer = LayerMask.NameToLayer("Wall");
		Block.floorLayer = LayerMask.NameToLayer("Floor");
				
		// let's compress all the block textures into a single tilesheet
		var atlas = new Texture2D(Block.pixelSize*100, Block.pixelSize*100);
		var boxes = atlas.PackTextures(blockTextures, 1, Block.pixelSize*blockTextures.Length*1);

		Block.tileWidth =  (float)Block.pixelSize / atlas.width;
		Block.tileHeight = (float)Block.pixelSize / atlas.height;


		/* There's some fiddliness here to do with texture bleeding and padding. We need a pixel
		 * of padding around each block to prevent white lines (possibly as a result of bilinear filtering?)
		 * but we then need to remove that padding in the UVs to prevent black lines. - mispy */
		var fracX = 1f/atlas.width;
		var fracY = 1f/atlas.height;

		for (var i = 0; i < Block.allTypes.Count; i++) {			
			var type = Block.allTypes[i];
			var box = boxes[i];

			type.upUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};

			type.downUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + Block.tileHeight - fracY)
			};

			type.rightUVs = new Vector2[] {
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};

			type.leftUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + Block.tileHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + Block.tileWidth - fracX, box.yMin + Block.tileHeight - fracY)
			};
		}

		Game.Prefab("BlockChunk").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
	}

	public static Vector2[] GetUVs(Block block) {		
		if (block.orientation == Orientation.up) {
			return block.type.upUVs;
		} else if (block.orientation == Orientation.down) {
			return block.type.downUVs;
		} else if (block.orientation == Orientation.left) {
			return block.type.leftUVs;
		} else if (block.orientation == Orientation.right) {
			return block.type.rightUVs;
		}

		throw new KeyNotFoundException();
	}

	public static bool IsType(Block block, string typeName) {
		return block != null && block.type == Block.types[typeName];
	}

	public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics.OverlapSphere(center, radius);

		List<Block> nearbyBlocks = new List<Block>();

		foreach (var hit in hits) {
			if (hit.gameObject.GetComponent<BoxCollider>() != null) {
				var ship = hit.attachedRigidbody.gameObject.GetComponent<Ship>();
				if (ship != null) {
					var block = ship.BlockAtWorldPos(hit.transform.position);
					if (block != null)
						nearbyBlocks.Add(block);
				}
			}
		}

		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.ship.BlockToWorldPos(block.pos)));
	}

	public static Block AtWorldPos(Vector2 worldPos, bool allowBlueprint = false) {
		foreach (var ship in Ship.allActive) {
			var block = ship.BlockAtWorldPos(worldPos);

			if (block != null)
				return block;

			block = ship.blueprint.BlockAtWorldPos(worldPos);

			if (block != null)
				return block;
		}

		return null;
	}

	public static Block BlueprintAtWorldPos(Vector2 worldPos) {
		foreach (var ship in Ship.allActive) {
			var block = ship.blueprint.BlockAtWorldPos(worldPos);

			if (block != null)
				return block;
		}

		return null;
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

	public int x {
		get { return pos.x; }
	}

	public int y {
		get { return pos.y; }
	}

	public float mass {
		get { return type.mass; }
	}

	public bool IsFilled {
		get { return type.scrapRequired == scrapContent; }
	}
		
	public bool IsActive {
		get { return ship != null && ship.blocks[pos] == this; }
	}

	public int CollisionLayer {
		get { return type.gameObject.layer; }
	}

	public bool IsBlueprint {
		get {
			return (this is BlueprintBlock);
		}
	}

	public float PercentFilled {
		get { 
			if (this.IsBlueprint) {
				return 1.0f;
			} else {
				return this.scrapContent / (float)this.type.scrapRequired; 
			}
		}
	}


	public void TakeDamage(int amount) {
		this.scrapContent -= amount;

		if (this.scrapContent <= 0) {
			ship.BreakBlock(this);
		} else {
			ship.blocks[pos] = this;
		}
	}

	public BlockType type;
	public float scrapContent;

	// these attributes relate to where the block is, rather
	// than what it does
	public Ship ship;
	public int index;
	public IntVector2 pos = new IntVector2();
	public Orientation orientation = Orientation.up;

	// relative to current chunk
	public int localX;
	public int localY;

	// copy constructor
	public Block(Block block) {
		this.type = block.type;
		this.orientation = block.orientation;
		this.scrapContent = block.scrapContent;
	}
		
	public Block(BlockType type) {
		this.type = type;
		this.scrapContent = type.scrapRequired;
	}

	public Block(BlueprintBlock blue) {
		this.type = blue.type;
		this.orientation = blue.orientation;
		this.scrapContent = 0;
	}
}

public class BlueprintBlock : Block {
	public BlueprintBlock(Block block) : base(block) {
	}

	public BlueprintBlock(BlockType type) : base(type) {
	}
}
