﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class BlockType {

	public GameObject prefab;
	public float mass;
	public string name;
	
	// precalculated uv coordinates for each orientation
	public Vector2[] upUVs;
	public Vector2[] downUVs;
	public Vector2[] leftUVs;
	public Vector2[] rightUVs;

	public BlockType(string name, float mass = 0.001f, GameObject prefab = null) {
		this.name = name;
		this.mass = mass;
		this.prefab = prefab;

		Block.manager.allTypes.Add(this);
		Block.types[name] = this;
	}
}

public class Block {
	public static Dictionary<string, BlockType> types = new Dictionary<string, BlockType>();

	public static int pixelSize = 32; // the size of a block in pixels
	public static float worldSize = 1f; // the size of the block in worldspace coordinates

	// size of a block as fraction of tilesheet size
	public static float tileWidth = 0; 
	public static float tileHeight = 0;
		
	public static int wallLayer;
	public static int floorLayer;

	public static GameObject wallColliderPrefab;
	public static GameObject floorColliderPrefab;

	// the core sequence of each block type sprite
	public static Texture2D[] sprites;

	public static BlockManager manager;
	
	public static void Setup(Texture2D[] blockSprites) {
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

	public static Block AtWorldPos(Vector2 worldPos) {
		foreach (var ship in Ship.allActive) {
			var block = ship.BlockAtWorldPos(worldPos);

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

	public Ship ship;
	public IntVector2 pos = new IntVector2();
	public BlockType type;
	public Orientation orientation = Orientation.up;

	public int collisionLayer;

	public int index;

	// copy constructor
	public Block(Block block) {
		this.type = block.type;
		this.orientation = block.orientation;
		this.collisionLayer = block.collisionLayer;
	}
		
	public Block(BlockType type) {
		this.type = type;

		if (type.name == "floor" || type.name == "console") {
			collisionLayer = LayerMask.NameToLayer("Floor");
		} else {
			collisionLayer = LayerMask.NameToLayer("Block");
		}
	}

	public static Block Deserialize(BlockData data) {
		var block = new Block(data.type);
		block.orientation = (Orientation)data.orientation;
		return block;
	}
	
	public BlockData Serialize() {
		var data = new BlockData();
		data.x = pos.x;
		data.y = pos.y;
		data.type = type;
		data.orientation = (int)orientation;
		return data;
	}
}

[Serializable]
public struct BlockData {
	public int x;
	public int y;
	public BlockType type;
	public int orientation;
}
