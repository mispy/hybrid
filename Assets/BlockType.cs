using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;
using System.Collections.Generic;

public class BlockComponent : PoolBehaviour {
    public Block block;
}


public class BlockType : MonoBehaviour {
    [Tooltip("The mass value of each block is added to the mass of a ship rigidBody.")]
    public float mass;

    public int scrapRequired = 30;

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

    /* Complex block specific functionality */
    [Tooltip("A complex block has its gameObject instantiated for every block instance. This is expensive!")]
    public bool isComplexBlock = false;
    public bool canBeFired = false;

    [HideInInspector]
    public SpriteRenderer renderer;

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
        renderer = GetComponent<SpriteRenderer>();
    }
}