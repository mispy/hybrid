using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class BlockAbility : PoolBehaviour {
    public HashSet<Block> blocks;
    public string key;

    public KeyCode keyCode {
        get {
            return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
        }
    }

    public virtual bool WorksWith(Block block) {
        return false;
    }
}

public class BlockComponent : PoolBehaviour {
    [NonSerialized]
    public Block block;
   

    public Blockform form;

    public virtual void OnNewBlock(Block block) { }
    public virtual void OnRealize() { }

    public string universalId {
        get {
            return block.pos.ToString() + ":" + block.layer.ToString() + ":" + GetType().Name;
        }
    }

    public bool isDirty {
        get {
            return syncVarDirtyBits != 0;
        }

        set {
            if (value == false)
                ClearAllDirtyBits();
            else
                SetDirtyBit(1);
        }
    }

    public virtual void OnSerialize(ExtendedBinaryWriter writer) {
        FieldInfo[] infos = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).OrderBy(fi => fi.Name).ToArray();
        
        foreach (var fi in infos) {
            if (fi.IsDefined(typeof(SyncVarAttribute), true)) {
                var val = fi.GetValue(this);
                if (fi.FieldType == typeof(bool))
                    writer.Write((bool)val);
            }
        }
    }

    public virtual void OnDeserialize(ExtendedBinaryReader reader) {
        FieldInfo[] infos = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).OrderBy(fi => fi.Name).ToArray();
        
        foreach (var fi in infos) {
            if (fi.IsDefined(typeof(SyncVarAttribute), true)) {
                if (fi.FieldType == typeof(bool))
                    fi.SetValue(this, reader.ReadBoolean());
            }
        }
    }
}

public class BlockType : MonoBehaviour {
    [Tooltip("The mass value of each block is added to the mass of a ship rigidBody.")]
    public float mass;

    static List<BlockType> all = new List<BlockType>();
    static Dictionary<string, BlockType> byId = new Dictionary<string, BlockType>();

    public static void LoadTypes() {
        foreach (var type in Game.LoadPrefabs<BlockType>("Blocks")) {
            type.tileable = Tile.tileables[type.name];
            BlockType.byId[type.name] = type;
            BlockType.all.Add(type);
        }
    }

    public static BlockType FromId(string id) {
        if (BlockType.byId.Keys.Count == 0)
            LoadTypes();

        return BlockType.byId[id];
    }

    public static List<BlockType> All {
        get {
            if (BlockType.all.Count == 0)
                LoadTypes();

            return BlockType.all.ToList();
        }
    }

    public string id { 
        get { return name; }
    }

    public string savePath {
        get { return "foo"; }
    }

    public int scrapRequired = 30;
    public float maxHealth = 1;
    public float damageBuffer = 0;

    [Header("Description")]
    [TextArea]
    public string descriptionHeader;
    [TextArea]
    public string descriptionBody;

    public Tileable tileable;

    public BlockLayer blockLayer;

    public bool canRotate = false;
    public bool canFitInsideWall = false;
	public bool canBlockSight = false;
    public bool canBlockFront = false;

    public BlockAbility[] abilities;

    /* Complex block specific functionality */
    public bool isComplexBlock = false;
    public bool showInMenu = false;
    public bool isWeapon = false;

    [Tooltip("Whether a block requires an attached console with an active crew member to function.")]
    public bool needsCrew = false;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    [NonSerialized]
    private BlockComponent[] _blockComponents;
    public BlockComponent[] blockComponents {
        get {
            if (_blockComponents == null) {
                _blockComponents = GetComponents<BlockComponent>();
            }
            return _blockComponents;
        }
    }

    public void Awake() {   
        Destroy(this);
    }
}