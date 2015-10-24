﻿using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;
using System.Collections.Generic;

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
    [HideInInspector]
    public Block block;
    [HideInInspector]
    public Blockform form;

    public virtual void OnNewBlock(Block block) { }
}


public class BlockType : MonoBehaviour, ISaveAsRef {
    [Tooltip("The mass value of each block is added to the mass of a ship rigidBody.")]
    public float mass;

    public static BlockType FromId(string id) {
        return Block.typeByName[id];
    }

    public string id { 
        get { return name; }
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
    [Tooltip("A complex block has its gameObject instantiated for every block instance. This is expensive!")]
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
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}