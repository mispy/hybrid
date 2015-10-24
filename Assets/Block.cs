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
    public int facing;
}

public static class BlockManager {
    public static BlockData Serialize(Block block) {
        var data = new BlockData();
        data.x = block.pos.x;
        data.y = block.pos.y;
        data.typeName = block.type.name;
        data.facing = (int)block.facing;
        return data;
    }

    public static Block Deserialize(BlockData data) {
        var block = new Block(Block.typeByName[data.typeName]);
        block.facing = (Facing)data.facing;
        return block;
    }
}

public class Block : ISaveBindable {
    public static Dictionary<string, BlockType> typeByName = new Dictionary<string, BlockType>();
    public static List<BlockType> allTypes = new List<BlockType>();
    
    public static int spaceLayer;
    public static int floorLayer;
    public static int wallLayer;

    public static GameObject wallColliderPrefab;
    public static GameObject floorColliderPrefab;
    
    public static string[] blockOrder = new string[] {
		"Sensors",
		"ShieldGenerator",
        "Floor",
        "Wall",
        "Console",
        "Thruster",
        "TractorBeam",
        "PowerNode",
        "TorpedoLauncher",
		"PlasmaTurret"
    };
    
    public static void Setup() {
        foreach (var type in Game.LoadPrefabs<BlockType>("Blocks")) {
            Block.typeByName[type.name] = type;
        }

        foreach (var name in blockOrder) {
            Block.allTypes.Add(Block.typeByName[name]);
        }
        
        foreach (var type in Block.typeByName.Values) {
            if (!Block.allTypes.Contains(type))
                Block.allTypes.Add(type);
            type.tileable = Tile.tileables[type.name];
        }

        Block.spaceLayer = LayerMask.NameToLayer("Space");
        Block.floorLayer = LayerMask.NameToLayer("Floor");
        Block.wallLayer = LayerMask.NameToLayer("Wall");
    }

    public override string ToString() {
        return String.Format("Block<{0}, {1}, {2}>", type.name, pos.x, pos.y);
    }
    
    public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
        var hits = Physics.OverlapSphere(center, radius);
        
        List<Block> nearbyBlocks = new List<Block>();
        
        foreach (var hit in hits) {
            if (hit.gameObject.GetComponent<BoxCollider>() != null) {
                var form = hit.attachedRigidbody.gameObject.GetComponent<Blockform>();
                if (form != null) {
                    var bp = form.WorldToBlockPos(hit.transform.position);

                    foreach (var block in form.blocks.BlocksAtPos(bp))
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

    public bool Is(string blockType) {
        return type.name == blockType;
    }

    public bool Is<T>() {
        return type.GetComponent<T>() != null;
    }

    public BlockType type;

    public bool isPowered = true;

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

	public bool IsDestroyed {
		get {
			return ship == null;
		}
	}

    public GameObject _gameObject;
    public GameObject gameObject {
        get {
            if (_gameObject == null && ship != null) {
                ship.form.RealizeBlock(this);
            }

            return _gameObject;
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
            if (facing == Facing.up) {
                return baseTile.up;
            } if (facing == Facing.right) {
                return baseTile.right;
            } if (facing == Facing.down) {
                return baseTile.down;
            } if (facing == Facing.left) {
                return baseTile.left;
            }

            return baseTile.up;
        }
    }

    public virtual void Savebind(ISaveBinder save) {
        save.BindRef("type", ref type);
        if (save is XMLSaveReader)
            MakeType(type);
        save.BindValue("position", ref pos);
        save.BindValue("facing", ref facing);
    }

    public float scrapContent;
    public float health;

    // these attributes relate to where the block is, rather
    // than what it does
    public Ship ship;
    public int index;
    public IntVector2 pos;
    public BlockLayer layer;
    public Facing facing = Facing.up;

    // relative to current chunk
    public int localX;
    public int localY;

    public Tileable tileable;

    public int Width {
        get { return tileable.tileWidth; }
    }

    public int Height {
        get { return tileable.tileHeight; }
    }

    public void MakeType(BlockType type) {
        this.type = type;
        this.scrapContent = type.scrapRequired;
        this.health = type.maxHealth;
        this.layer = type.blockLayer;
        this.tileable = type.tileable;
    }

    public Block(BlockType type) {
        MakeType(type);
    }

    // copy constructor
    public Block(Block block) : this(block.type) {
        this.facing = block.facing;
    }

    public Block(BlueprintBlock blue) : this(blue.type) {
        this.facing = blue.facing;
    }
}

public class BlueprintBlock : Block {
	// Check if a blueprint block already matches the constructed block
	public static bool Matches(BlueprintBlock blue, Block block) {
		if (blue == null && block == null) return true;
		if (blue == null && block != null) return false;
		if (blue != null && block == null) return false;

		if (blue.type != block.type || blue.facing != block.facing)
			return false;

		return true;
	}

    public static BlueprintBlock Make(string typeName) {
        return new BlueprintBlock(Block.typeByName[typeName]);
    }

    public BlueprintBlock(Block block) : base(block) { }
    public BlueprintBlock(BlockType type) : base(type) { }
}
