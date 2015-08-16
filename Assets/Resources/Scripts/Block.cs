using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tileable {
	public string name;
	public int tileWidth;
	public int tileHeight;
	public Tile[,] tiles;

	public Tileable(int width, int height) {
		tileWidth = width;
		tileHeight = height;
		tiles = new Tile[width, height];
	}
}

public class Tile {
	public static List<Texture2D> textures = new List<Texture2D>();
	public static List<Tile> allTiles = new List<Tile>();
	public static Dictionary<string, Tileable> tileables = new Dictionary<string, Tileable>();

	public static int pixelSize = 32; // the size of a tile in pixels
	public static float worldSize = 1f; // the size of a tile in worldspace coordinates
	
	// size of a tile as fraction of tilesheet size
	public static float fracWidth = 0; 
	public static float fracHeight = 0;

	public static void Setup() {
		foreach (var texture in Game.LoadTextures("Tileables")) {
			var tileWidth = texture.width / Tile.pixelSize;
			var tileHeight = texture.height / Tile.pixelSize;
			var tileable = new Tileable(tileWidth, tileHeight);

			if (texture.width == Tile.pixelSize && texture.height == Tile.pixelSize) {
				var tile = new Tile();
				tile.texture = texture;
				tileable.tiles[0, 0] = tile;
				Tile.allTiles.Add(tile);
			} else {
				for (var x = 0; x < tileWidth; x++) {
					for (var y = 0; y < tileHeight; y++) {
						Color[] pixels = texture.GetPixels(x*Tile.pixelSize, y*Tile.pixelSize, Tile.pixelSize, Tile.pixelSize);
						var tileTex = new Texture2D(Tile.pixelSize, Tile.pixelSize, texture.format, false);
						tileTex.SetPixels(pixels);
						tileTex.Apply();
						var tile = new Tile();
						tile.texture = tileTex;
						tileable.tiles[x, y] = tile;
						//Debug.LogFormat("{0} {1} {2} {3} {4}", x, y, tileWidth, tileHeight, texture.name);
						Tile.allTiles.Add(tile);
					}
				}
			}

			tileables[texture.name] = tileable;
		}
		
		// let's compress all the textures into a single tilesheet
		Texture2D[] textures = allTiles.Select(type => type.texture).ToArray();
		var atlas = new Texture2D(pixelSize*100, pixelSize*100);
		var boxes = atlas.PackTextures(textures, 1, pixelSize*textures.Length*1);
		
		Tile.fracWidth = (float)pixelSize / atlas.width;
		Tile.fracHeight = (float)pixelSize / atlas.height;
		
		/* There's some fiddliness here to do with texture bleeding and padding. We need a pixel
		 * of padding around each tile to prevent white lines (possibly as a result of bilinear filtering?)
		 * but we then need to remove that padding in the UVs to prevent black lines. - mispy */
		var fracX = 1f/atlas.width;
		var fracY = 1f/atlas.height;
		
		for (var i = 0; i < allTiles.Count; i++) {			
			var tile = allTiles[i];
			var box = boxes[i];

			tile.upUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};
			
			tile.downUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY)
			};
			
			tile.rightUVs = new Vector2[] {
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};
			
			tile.leftUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY)
			};
		}
		
		Game.Prefab("BlockChunk").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
	}

	public Texture2D texture;

	// precalculated uv coordinates for each orientation
	[HideInInspector]
	public Vector2[] upUVs;
	[HideInInspector]
	public Vector2[] downUVs;
	[HideInInspector]
	public Vector2[] leftUVs;
	[HideInInspector]
	public Vector2[] rightUVs;
}

public class Block {
	public static Dictionary<string, BlockType> types = new Dictionary<string, BlockType>();
	public static List<BlockType> allTypes = new List<BlockType>();
		
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

			type.tile = Tile.tileables[type.name].tiles[0, 0];
		}

		Block.wallLayer = LayerMask.NameToLayer("Wall");
		Block.floorLayer = LayerMask.NameToLayer("Floor");				
	}

	public static Vector2[] GetUVs(Block block) {		
		if (block.orientation == Orientation.up) {
			return block.type.tile.upUVs;
		} else if (block.orientation == Orientation.down) {
			return block.type.tile.downUVs;
		} else if (block.orientation == Orientation.left) {
			return block.type.tile.leftUVs;
		} else if (block.orientation == Orientation.right) {
			return block.type.tile.rightUVs;
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

	public float powerConsumeRate {
		get {
			return type.powerConsumeRate;
		}
	}

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
