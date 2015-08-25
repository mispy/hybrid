using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipCollision : PoolBehaviour {
	Ship ship;
	BlockMap blocks;
	GameObject collidersObj;
	public Dictionary<IntVector2, Collider> colliders { get; private set; }

	void Awake() {
		blocks = GetComponentInChildren<BlockMap>();
		colliders = new Dictionary<IntVector2, Collider>();
		ship = GetComponent<Ship>();

		var obj = Pool.For("Holder").TakeObject();
		obj.name = "Colliders";
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.transform.rotation = transform.rotation;
		obj.SetActive(true);
		collidersObj = obj;

		foreach (var block in blocks.AllBlocks) {
			if (blocks.IsCollisionEdge(block.pos)) {
				AddCollider(block.pos, block.CollisionLayer);
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
		
		Collider collider;
		if (collisionLayer == Block.wallLayer)
			collider = Pool.For("WallCollider").Take<Collider>();
		else
			collider = Pool.For("FloorCollider").Take<Collider>();
		collider.transform.SetParent(collidersObj.transform);
		collider.transform.localPosition = ship.BlockToLocalPos(bp);
		collider.transform.rotation = transform.rotation;
		colliders[bp] = collider;
		collider.gameObject.SetActive(true);
		
		Profiler.EndSample();
	}
	
	void UpdateCollider(IntVector2 pos) {
		Profiler.BeginSample("UpdateCollider");
		var collisionLayer = blocks.CollisionLayer(pos);
		
		var hasCollider = colliders.ContainsKey(pos);
		var isEdge = blocks.IsCollisionEdge(pos);
		
		if (hasCollider && (!isEdge || colliders[pos].gameObject.layer != collisionLayer)) {
			colliders[pos].gameObject.SetActive(false);
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
