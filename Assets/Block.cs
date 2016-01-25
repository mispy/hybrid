using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BlockLayer {
    Base = 0,
    Top = 1
}

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class Block {
    public static int spaceLayer;
    public static int floorLayer;
    public static int wallLayer;

    public static GameObject wallColliderPrefab;
    public static GameObject floorColliderPrefab;
    
    static Block() {
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
        
        return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.ship.BlockToWorldPos(block.pos)));
    }
    
    public static IEnumerable<Block> AtWorldPos(Vector2 worldPos, bool allowBlueprint = false) {
        foreach (var form in Game.activeSector.blockforms) {
            foreach (var block in form.BlocksAtWorldPos(worldPos))
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
        
    public bool isBlueprint {
        get {
            return _health == 0;
        }
    }

    public bool isDamaged {
        get {
            return _health < type.maxHealth;
        }
    }

	public bool isDestroyed {
		get {
            return _health == 0;
		}
	}

    public CrewMind mind;

    public GameObject gameObject = null;


    public float PercentFilled {
        get { 
            if (this.isBlueprint) {
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

    public float scrapContent;

    private float _health;
    public float health {
        get {
            return _health;
        }

        set {
            var oldHealth = _health;
            _health = Mathf.Clamp(value, 0, type.maxHealth);
            if (ship == null) return;

            if (_health == 0 && oldHealth != 0) {
                ship.blocks.BlockDestroyed(this);
            } else if (_health != 0 && oldHealth == 0) {
                ship.blocks.BlockRepaired(this);   
            }

            if (_health != oldHealth) 
                ship.blocks.HealthUpdate(this);                
        }
    }

    // these attributes relate to where the block is, rather
    // than what it does
    public Blockform ship;
    public int index;
    public IntVector2 pos;
    public IntVector3 blockPos {
        get {
            return new IntVector3(pos.x, pos.y, (int)layer);
        }

        set {
            pos = new IntVector2(value.x, value.y);
            layer = (BlockLayer)value.z;
        }
    }
    public Vector2 worldPos {
        get {
            return ship.BlockToWorldPos(this);
        }
    }
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

        foreach (var comp in type.blockComponents) {
            comp.OnNewBlock(this);
        }
    }

    public T GetBlockComponent<T>() {
        if (gameObject == null) return default(T);
        return gameObject.GetComponent<T>();
    }

    public Block() { }

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
	public static bool Matches(Block blue, Block block) {
		if (blue == null && block == null) return true;
		if (blue == null && block != null) return false;
		if (blue != null && block == null) return false;

		if (blue.type != block.type || blue.facing != block.facing)
			return false;

		return true;
	}

    public static BlueprintBlock Make(string typeName) {
        return new BlueprintBlock(BlockType.FromId(typeName));
    }

    public BlueprintBlock(Block block) : base(block) { }
    public BlueprintBlock(BlockType type) : base(type) { }
}
