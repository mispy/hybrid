using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipCollision : PoolBehaviour {
    [SerializeField]
    Blockform form;
    [SerializeField]
    BlockMap blocks;
    [SerializeField]
    Transform collidersHolder;

    public Dictionary<IntVector2, Collider> colliders { get; private set; }


    void Awake() {
        form = GetComponent<Blockform>();

        collidersHolder = Pool.For("Holder").Attach<Transform>(transform);
        collidersHolder.name = "Colliders";
        collidersHolder.transform.localScale *= Tile.worldSize;
    }

    void OnEnable() {
        colliders = new Dictionary<IntVector2, Collider>();

        blocks = form.blocks;

        foreach (var pos in blocks.FilledPositions) {
            if (blocks.IsCollisionEdge(pos)) {
                AddCollider(pos, blocks.CollisionLayer(pos));
            }
        }

        blocks.OnBlockAdded += OnBlockUpdate;
        blocks.OnBlockRemoved += OnBlockUpdate;
    }

    void OnBlockUpdate(Block block) {
        UpdateCollision(block.pos);
    }
    
    void AddCollider(IntVector2 bp, int collisionLayer) {
        Profiler.BeginSample("AddCollider");
        
        var pool = collisionLayer == Block.wallLayer ? "WallCollider" : "FloorCollider";
        var collider = Pool.For(pool).Attach<Collider>(collidersHolder.transform);
        collider.transform.localPosition = form.BlockToLocalPos(bp);
        colliders[bp] = collider;
        collider.gameObject.SetActive(true);
        
        Profiler.EndSample();
    }
    
    void UpdateCollider(IntVector2 pos) {
        Profiler.BeginSample("UpdateCollider");
        var collisionLayer = blocks.CollisionLayer(pos);
        
        var hasCollider = colliders.ContainsKey(pos);
        var isEdge = blocks.IsCollisionEdge(pos);

        if (hasCollider && colliders[pos] == null) {
            colliders.Remove(pos);
            hasCollider = false;
        }

        if (hasCollider && (!isEdge || colliders[pos].gameObject.layer != collisionLayer)) {
            Pool.Recycle(colliders[pos].gameObject);
            colliders.Remove(pos);
            hasCollider = false;
        }
        
        if (!hasCollider && isEdge) {
            AddCollider(pos, collisionLayer);
        }
    
        Profiler.EndSample();
    }

    
    void UpdateCollision(IntVector2 pos) {
        foreach (var other in IntVector2.Neighbors(pos)) {
            UpdateCollider(other);
        }
        
        UpdateCollider(pos);
    }
}
