using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BlockLayer {
	Base = 0,
	Top = 1
}

[Serializable]
public struct BlockData {
	public int x;
	public int y;
	public string typeName;
	public int orientation;
}

public static class BlockManager {
	public static BlockData Serialize(Block block) {
		var data = new BlockData();
		data.x = block.pos.x;
		data.y = block.pos.y;
		data.typeName = block.type.name;
		data.orientation = (int)block.orientation;
		return data;
	}

	public static Block Deserialize(BlockData data) {
		var block = new Block(Block.typeByName[data.typeName]);
		block.orientation = (Orientation)data.orientation;
		return block;
	}
}

public class Block {
	public static Dictionary<Type, BlockType> types = new Dictionary<Type, BlockType>();
	public static Dictionary<string, BlockType> typeByName = new Dictionary<string, BlockType>();
	public static List<BlockType> allTypes = new List<BlockType>();
	
	public static int spaceLayer;
	public static int floorLayer;
	public static int wallLayer;

	public static GameObject wallColliderPrefab;
	public static GameObject floorColliderPrefab;
	
	public static string[] blockOrder = new string[] {
		"Floor",
		"Wall",
		"Console",
		"Thruster",
		"TractorBeam",
		"InertiaStabilizer",
		"PowerNode",
		"TorpedoLauncher"
	};
	
	public static void Setup() {
		foreach (var type in Game.LoadPrefabs<BlockType>("Blocks")) {
			Block.types[type.GetType()] = type;
			Block.typeByName[type.GetType().Name] = type;
		}

		foreach (var name in blockOrder) {
			Block.allTypes.Add(Block.typeByName[name]);
		}
		
		foreach (var type in Block.types.Values) {
			if (!Block.allTypes.Contains(type))
				Block.allTypes.Add(type);
			type.tileable = Tile.tileables[type.GetType().Name];
		}

		Block.spaceLayer = LayerMask.NameToLayer("Space");
		Block.floorLayer = LayerMask.NameToLayer("Floor");				
		Block.wallLayer = LayerMask.NameToLayer("Wall");
	}
	
	public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics.OverlapSphere(center, radius);
		
		List<Block> nearbyBlocks = new List<Block>();
		
		foreach (var hit in hits) {
			if (hit.gameObject.GetComponent<BoxCollider>() != null) {
				var form = hit.attachedRigidbody.gameObject.GetComponent<Blockform>();
				if (form != null) {
					var bp = form.WorldToBlockPos(hit.transform.position);

					foreach (var block in form.blocks[bp])
						nearbyBlocks.Add(block);
				}
			}
		}
		
		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.ship.form.BlockToWorldPos(block.pos)));
	}
	
	public static IEnumerable<Block> AtWorldPos(Vector2 worldPos, bool allowBlueprint = false) {
		foreach (var form in Game.activeSector.blockforms) {
			foreach (var block in form.BlocksAtWorldPos(worldPos))
				yield return block;

			foreach (var block in form.blueprint.BlocksAtWorldPos(worldPos))
				yield return block;
		}
	}
	
	public static IEnumerable<Block> BlueprintAtWorldPos(Vector2 worldPos) {
		foreach (var form in Game.activeSector.blockforms) {
			foreach (var block in form.blueprint.BlocksAtWorldPos(worldPos))
				yield return block;
		}
	}
	
	public static IEnumerable<Block> FromHits(RaycastHit[] hits) {
		foreach (var hit in hits) {
			var form = hit.rigidbody.gameObject.GetComponent<Blockform>();
			if (form != null) {
				foreach (var block in form.BlocksAtWorldPos(hit.collider.transform.position))
					yield return block;
			}
		}
	}

	public static bool Is<T>(Block block) {
		return block != null && block.type is T;
	}

	public BlockType type;

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
		get { return ship != null && ship.blocks[pos, layer] == this; }
	}

	public int CollisionLayer {
		get { return type.gameObject.layer; }
	}

	public bool IsBlueprint {
		get {
			return (this is BlueprintBlock);
		}
	}

	public GameObject gameObject;

	public float PercentFilled {
		get { 
			if (this.IsBlueprint) {
				return 1.0f;
			} else {
				return this.scrapContent / (float)this.type.scrapRequired; 
			}
		}
	}

	public Tile Tile {
		get {
			var tileable = Tile.tileables[this.type.name];
			var baseTile = tileable.tiles[0,0];
			if (orientation == Orientation.up) {
				return baseTile.up;
			} if (orientation == Orientation.right) {
				return baseTile.right;
			} if (orientation == Orientation.down) {
				return baseTile.down;
			} if (orientation == Orientation.left) {
				return baseTile.left;
			}

			return baseTile.up;
		}
	}

	public void TakeDamage(int amount) {
		this.scrapContent -= amount;

		if (this.scrapContent <= 0) {
			ship.form.BreakBlock(this);
		}
	}


	public float scrapContent;

	// these attributes relate to where the block is, rather
	// than what it does
	public Ship ship;
	public int index;
	public IntVector2 pos = new IntVector2();
	public BlockLayer layer;
	public Orientation orientation = Orientation.up;

	// relative to current chunk
	public int localX;
	public int localY;

	public int Width {
		get { return type.tileable.tileWidth; }
	}

	public int Height {
		get { return type.tileable.tileHeight; }
	}

	public static Block Make<T>() {
		return new Block(Block.types[typeof(T)]);
	}

	public Block(BlockType type) {
		this.type = type;
		this.scrapContent = type.scrapRequired;
		this.layer = type.blockLayer;
	}

	// copy constructor
	public Block(Block block) : this(block.type) {
		this.type = block.type;
		this.orientation = block.orientation;
	}

	public Block(BlueprintBlock blue) : this(blue.type) {
		this.type = blue.type;
		this.orientation = blue.orientation;
	}
}

public class BlueprintBlock : Block {
	public static BlueprintBlock Make<T>() {
		return new BlueprintBlock(Block.types[typeof(T)]);
	}

	public BlueprintBlock(Block block) : base(block) { }
	public BlueprintBlock(BlockType type) : base(type) { }
}
