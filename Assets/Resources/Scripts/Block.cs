using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block {
	public static Dictionary<string, BlockDef> types = new Dictionary<string, BlockDef>();
	public static List<BlockDef> allTypes = new List<BlockDef>();
		
	public static int wallLayer;
	public static int floorLayer;

	public static GameObject wallColliderPrefab;
	public static GameObject floorColliderPrefab;

	public static string[] blockOrder = new string[] {
		"Floor",
		"Wall",
		"Console",
		"Thruster",
		"TractorBeam",
		"InertiaStabilizer",
		"ShieldGenerator",
		"TorpedoLauncher"
	};

	public static void Setup() {
		foreach (var type in Game.LoadPrefabs<BlockDef>("Blocks")) {
			type.name = type.gameObject.name;
			Block.types[type.name] = type;
		}

		foreach (var name in blockOrder) {
			Block.allTypes.Add(Block.types[name]);
		}

		foreach (var type in Block.types.Values) {
			if (!Block.allTypes.Contains(type))
				Block.allTypes.Add(type);

			type.baseTile = Tile.tileables[type.name].tiles[0, 0];
		}

		Block.wallLayer = LayerMask.NameToLayer("Wall");
		Block.floorLayer = LayerMask.NameToLayer("Floor");				
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

	public BlockDef type;

	public bool Is<T>() {
		return type.GetComponent<T>() != null;
	}

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
			ship.BreakBlock(this);
		} else {
			ship.blocks[pos] = this;
		}
	}

	public float scrapContent;

	public float powerConsumeRate {
		get {
			return type.powerConsumeRate;
		}
	}

	public int collisionLayer;

	// these attributes relate to where the block is, rather
	// than what it does
	public Ship ship;
	public int index;
	public IntVector2 pos = new IntVector2();
	public Orientation orientation = Orientation.up;

	// relative to current chunk
	public int localX;
	public int localY;

	public Block() {
	}

	// copy constructor
	public Block(Block block) {
		this.type = block.type;
		this.orientation = block.orientation;
		this.scrapContent = block.scrapContent;
	}
		
	public Block(BlockDef type) {
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

	public BlueprintBlock(BlockDef type) : base(type) {
	}
}
