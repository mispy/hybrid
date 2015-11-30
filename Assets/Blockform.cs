using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Blockform))]
public class ShipEditor : Editor {
    public override void OnInspectorGUI() {
        Blockform form = (Blockform)target;
        
        if (GUILayout.Button("Save To Template")) {
            var path = "Assets/Resources/Ships/" + target.name + ".prefab";

            var prefab = Resources.Load(path);

            if (prefab == null) {
                var template = Pool.For("ShipTemplate").Attach<ShipTemplate2>(Game.state.transform);
                PrefabUtility.CreatePrefab(path, template.gameObject);
            } else {
                var template = (prefab as GameObject).GetComponent<ShipTemplate2>();
                template.Fill(form);
                PrefabUtility.ReplacePrefab(template.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }

        }


        DrawDefaultInspector();
    }
}
#endif

public class Blockform : PoolBehaviour {
    [ReadOnlyAttribute]
    public Bounds localBounds = new Bounds();
    [ReadOnlyAttribute]
    public Vector3 centerOfMass;
    [ReadOnlyAttribute]
    public List<CrewBody> maglockedCrew = new List<CrewBody>();
    [ReadOnlyAttribute]
    public HashSet<Block> poweredWeapons = new HashSet<Block>();

    public BlockMap blocks { get; private set; }
    public Blueprint blueprint { get; private set; }    
    public TileRenderer tiles { get; private set; }
    public Rigidbody rigidBody { get; private set; }
    public InteriorFog fog { get; private set; }
    public Transform blockComponentHolder { get; private set; }
    public SpacePather pather { get; private set; }
    public BoxCollider box { get; private set; }
    public ShipDamage damage { get; private set; }
    [HideInInspector]
    public Shields shields;

    private bool needsMassUpdate = true;

    public static IEnumerable<Blockform> ClosestTo(Vector2 worldPos) {
        return Game.activeSector.blockforms.OrderBy((form) => Vector2.Distance(form.transform.position, worldPos));
    }
    
    public static Blockform AtWorldPos(Vector2 worldPos) {
        foreach (var form in Game.activeSector.blockforms) {
            var blockPos = form.WorldToBlockPos(worldPos);
            if (form.blocks[blockPos, BlockLayer.Base] != null) {

                return form;
            }
        }
        
        return null;
    }

    public static Blockform FromTemplate(ShipTemplate2 template) {
        var ship = Pool.For("Blockform").Attach<Blockform>(Game.activeSector.contents, false);
        template.blocks.ReadBlockData();

        Debug.Assert(template.blocks.allBlocks.Count > 0, "Expected template.blocks.allBlocks.Count > 0 for " + template.name);
        ship.name = template.name;
        ship.Initialize(template);

        ship.gameObject.SetActive(true);
        return ship;
    }

    List<BlockComponent> dirtyComps = new List<BlockComponent>();

    public override void OnSerialize(ExtendedBinaryWriter binary, bool initial) {               
        if (!initial && dirtyComps.Count == 0) return;

        Debug.Log("OnSerialize");
        if (initial) {
            binary.Write(blocks.allBlocks.Count);
            foreach (var block in blocks.allBlocks) {
                binary.Write(block);
            }                  
        }
    }

    public override void OnDeserialize(ExtendedBinaryReader binary, bool initial) {
        Debug.Log("OnDeserialize");
        if (!initial) return;

        blocks = Pool.For("BlockMap").Attach<BlockMap>(transform);

        var count = binary.ReadInt32();
        for (var i = 0; i < count; i++) {
            var block = binary.ReadBlock();
            blocks[block.pos, block.layer] = block;
        }        
    }

	public float width {
		get {
			return (blocks.maxX - blocks.minX) * Tile.worldSize;
		}
	}

	public float height {
		get {
			return (blocks.maxY - blocks.minY) * Tile.worldSize;
		}
	}

	public float length {
		get {
			return Mathf.Max(width, height);
		}
	}

	public bool hasActiveShields {
		get {
			return shields != null && shields.isActive;
		}
	}

	public bool canFoldJump {
		get {
			return Game.activeSector.IsOutsideBounds(transform.position);
		}
	}

    public Dictionary<Type, HashSet<BlockComponent>> blockCompCache;

    public IEnumerable<T> GetBlockComponents<T>() {
        if (!blockCompCache.ContainsKey(typeof(T)))
            yield break;

        foreach (var comp in blockCompCache[typeof(T)]) {
            yield return (T)Convert.ChangeType(comp, typeof(T));
        }
    }
    
    public bool HasBlockComponent<T>() {
        return GetBlockComponents<T>().Count() > 0;
    }

    public void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        tiles = GetComponent<TileRenderer>();
        damage = GetComponent<ShipDamage>();
        box = Pool.For("BoundsCollider").Attach<BoxCollider>(transform);
        blockCompCache = new Dictionary<Type, HashSet<BlockComponent>>();
        box.isTrigger = true;
    }

    public void Initialize(ShipTemplate2 template) {
        blocks = Pool.For("BlockMap").Attach<BlockMap>(transform);

        //blueprint = Pool.For("Blueprint").Attach<Blueprint>(transform);
        //blueprint.Initialize();
        //blueprint.tiles.DisableRendering();

        foreach (var block in template.blocks.allBlocks) {
            blocks[block.pos, block.layer] = new Block(block);
            //blueprint.blocks[block.pos, block.layer] = new Block(block);
        }

    }

    void Start() {               
        blockComponentHolder = Pool.For("Holder").Attach<Transform>(transform);
        blockComponentHolder.name = "BlockComponents";
        blocks.OnBlockRemoved += OnBlockRemoved;
        blocks.OnBlockAdded += OnBlockAdded;
        
        Debug.Assert(blocks.allBlocks.Count() > 0, "Expected allBlocks.Count() > 0");
        foreach (var block in blocks.allBlocks) {
            OnBlockAdded(block);
        }
        
        Game.activeSector.blockforms.Add(this);
        InvokeRepeating("UpdateMass", 0f, 0.5f);
    }
    
    void OnDisable() {
        blockCompCache.Clear();

        Pool.Recycle(blockComponentHolder.gameObject);

        blocks.OnBlockRemoved -= OnBlockRemoved;
        blocks.OnBlockAdded -= OnBlockAdded;

        Game.activeSector.blockforms.Remove(this);
	}

    public void OnDestroy() {
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }

    public void ReceiveImpact(Rigidbody fromRigid, Block block) {
        //var impactVelocity = rigidBody.velocity - fromRigid.velocity;
        //var impactForce = impactVelocity.magnitude * fromRigid.mass;
        //if (impactForce < 5) return;
        
        // break it off into a separate fragment
        //BreakBlock(block);
    }

	public Facing GetSideWithMostWeapons() {
		var sideCount = new Dictionary<Facing, int>();
		sideCount[Facing.up] = 0;
		foreach (var launcher in GetBlockComponents<ProjectileLauncher>()) {
			if (!sideCount.ContainsKey(launcher.block.facing))
				sideCount[launcher.block.facing] = 0;
			sideCount[launcher.block.facing] += 1;
		}
		return sideCount.OrderBy((kv) => -kv.Value).First().Key;
	}


    public void OnBlockRemoved(Block oldBlock) {
        Profiler.BeginSample("OnBlockRemoved");
        
        //if (oldBlock.layer == BlockLayer.Base)
        //    this.size -= 1;

        UpdateBlock(oldBlock);       

        if (oldBlock._gameObject != null) {
            foreach (var comp in oldBlock.gameObject.GetComponents<BlockComponent>()) {
                if (blockCompCache.ContainsKey(comp.GetType()))
                    blockCompCache[comp.GetType()].Remove(comp);
            }
        }

        if (oldBlock._gameObject != null)
            Pool.Recycle(oldBlock.gameObject);

        Profiler.EndSample();
    }

/*    [ClientRpc]
    public void RpcSetBlock(IntVector2 bp, BlockLayer layer, string typeId, Facing facing) {
        if (NetworkServer.active) return;

        var block = new Block(BlockType.FromId(typeId));
        block.facing = facing;
        blocks[bp, layer] = block;
    }
    
    [ClientRpc]
    public void RpcDelBlock(IntVector2 bp, BlockLayer layer) {
        blocks[bp, layer] = null;
    }*/

/*    [ClientRpc]
    public void RpcSetup() {
        if (NetworkServer.active) return;
    }*/

    public void OnBlockAdded(Block newBlock) {
        newBlock.ship = this;
        UpdateBlock(newBlock);
        
        if (newBlock.type.isComplexBlock) {
            RealizeBlock(newBlock);
        }    
    }

	public void UpdateBounds() {
        localBounds.center = blocks.boundingRect.center * Tile.worldSize;
        localBounds.size = blocks.boundingRect.size * Tile.worldSize + new Vector2(Tile.worldSize, Tile.worldSize);
        box.transform.localPosition = localBounds.center;
        box.size = localBounds.size;
	}
    
    public void UpdateBlock(Block block) {
        if (block.mass != 0)
            needsMassUpdate = true;
        
		UpdateBounds();
    }

    public void RealizeBlock(Block block) {
        Vector2 worldOrient = transform.TransformVector((Vector2)block.facing);

        var obj = Pool.For(block.type.gameObject).Attach<Transform>(blockComponentHolder, false);

        obj.transform.SetParent(blockComponentHolder);
        obj.transform.position = BlockToWorldPos(block);
        obj.transform.up = worldOrient;
        obj.transform.localScale *= Tile.worldSize;
        
        block._gameObject = obj.gameObject;
        
        foreach (var comp in obj.GetComponents<BlockComponent>()) {
            comp.block = block;
            comp.form = this;
            comp.OnRealize();

            if (!blockCompCache.ContainsKey(comp.GetType()))
                blockCompCache[comp.GetType()] = new HashSet<BlockComponent>();
            blockCompCache[comp.GetType()].Add(comp);

            comp.guid = new GUID(guid.ToString() + ":" + block.pos.ToString() + ":" + block.layer.ToString() + ":" + comp.GetType().Name);
            SpaceNetwork.Register(comp);
        }
        
        if (!block.type.isComplexBlock)
            obj.GetComponent<SpriteRenderer>().enabled = false;

        obj.gameObject.SetActive(true);
    }
    
    public void UpdateMass() {        
        if (!needsMassUpdate) return;
        
        var totalMass = 0.0f;
        var avgPos = new IntVector2(0, 0);
        
        foreach (var block in blocks.allBlocks) {
			avgPos.x += block.pos.x;
			avgPos.y += block.pos.y;
            totalMass += block.mass;
        }
        
        rigidBody.mass = totalMass;
        
        avgPos.x /= blocks.allBlocks.Count;
        avgPos.y /= blocks.allBlocks.Count;
        centerOfMass = BlockToLocalPos(avgPos);
        rigidBody.centerOfMass = centerOfMass;
        
        needsMassUpdate = false;
    }

    public float RotateTowards(Vector2 worldPos, Facing rot) {
        var dir = (worldPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
        var currentAngle = transform.localEulerAngles.z;

		if (rot == Facing.up) {
			angle -= 90;
		} else if (rot == Facing.right) {
		} else if (rot == Facing.left) {
			angle += 90;
		} else if (rot == Facing.down) {
			angle += 180;
		}
        
        if (Math.Abs(360+angle - currentAngle) < Math.Abs(angle - currentAngle)) {
            angle = 360+angle;
        }
        
        if (angle > currentAngle + 15) {
            FireAttitudeThrusters(Facing.right);
        } else if (angle < currentAngle - 15) {
            FireAttitudeThrusters(Facing.left);
        }
        
		return angle-currentAngle;
    }

    public float RotateTowards(Vector2 worldPos) {
        return RotateTowards(worldPos, Facing.up);
    }
    
    public void MoveTowards(Vector3 worldPos) {
		var angle = Vector3.Angle(transform.TransformDirection(Vector2.up), (worldPos-transform.position).normalized);
		if (angle < 60) {
			FireThrusters(Facing.down);
		}

		if (angle > 180 - 60) {
			FireThrusters(Facing.up);
		}
        /*var localDir = transform.InverseTransformDirection((worldPos - (Vector2)transform.position).normalized);
        var orient = Util.cardinalToOrient[Util.Cardinalize(localDir)];
        FireThrusters((Orientation)(-(int)orient));*/
    }

	public void AvoidCollision() {
        if (this == Game.playerShip) return;

		foreach (var form in Util.ShipsInRadius(transform.position, length*2)) {
			if (form == this) continue;

			var local = transform.InverseTransformPoint(form.transform.position);
			if (local.x > 0)
				FireThrusters(Facing.right);
			if (local.x < 0)
				FireThrusters(Facing.left);
			if (local.y > 0)
				FireThrusters(Facing.up);
			if (local.y < 0)
				FireThrusters(Facing.down);
		}
	}
	
	public void FollowPath(List<Vector2> path) {
		var target = path[0];
        RotateTowards(target);
		MoveTowards(target);
		//else
			//FireThrusters(Orientation.up);
	}
    
    public IntVector2 WorldToBlockPos(Vector2 worldPos) {
        return LocalToBlockPos(transform.InverseTransformPoint(worldPos));
    }
    
    public IntVector2 LocalToBlockPos(Vector3 localPos) {
        // remember that blocks go around the center point of the center block at [0,0]        
        return new IntVector2(Mathf.FloorToInt((localPos.x + Tile.worldSize/2.0f) / Tile.worldSize),
                              Mathf.FloorToInt((localPos.y + Tile.worldSize/2.0f) / Tile.worldSize));
    }
    
    
    public Vector2 BlockToLocalPos(IntVector2 blockPos) {
        return new Vector2(blockPos.x*Tile.worldSize, blockPos.y*Tile.worldSize);
    }
    
    public Vector2 BlockToLocalPos(Block block) {
        var centerX = block.pos.x + block.Width/2.0f;
        var centerY = block.pos.y + block.Height/2.0f;
        return new Vector2(centerX * Tile.worldSize - Tile.worldSize/2.0f, centerY * Tile.worldSize - Tile.worldSize/2.0f);
    }
    
    public Vector2 BlockToWorldPos(IntVector2 blockPos) {
        return transform.TransformPoint(BlockToLocalPos(blockPos));
    }
    
    public Vector2 BlockToWorldPos(Block block) {
        return transform.TransformPoint(BlockToLocalPos(block));
    }
    
    public IEnumerable<Block> BlocksAtWorldPos(Vector2 worldPos) {
		return blocks.BlocksAtPos(WorldToBlockPos(worldPos));
    }
    
	public IEnumerable<Block> BlocksInLocalRadius(Vector2 localPos, float radius) {
		IntVector2 blockPos = LocalToBlockPos(localPos);
		int blockRadius = Mathf.RoundToInt(radius/Tile.worldSize);

		for (var i = -blockRadius; i <= blockRadius; i++) {
			for (var j = -blockRadius; j <= blockRadius; j++) {
				var pos = new IntVector2(blockPos.x + i, blockPos.y + j);
				if (IntVector2.Distance(pos, blockPos) <= blockRadius) {
					foreach (var block in blocks.BlocksAtPos(pos))
						yield return block;
				}
			}
		}
	}

    public IEnumerable<Block> BlocksInWorldRadius(Vector2 worldPos, float radius) {
        return BlocksInLocalRadius(transform.InverseTransformPoint(worldPos), radius);
    }

    public void FireThrusters(Facing dir) {
        foreach (var thruster in GetBlockComponents<Thruster>()) {
            if (thruster.block.facing == dir)
                thruster.Fire();
        }
    }
    
    public void FireAttitudeThrusters(Facing dir) {
        foreach (var thruster in GetBlockComponents<Thruster>()) {
            if (thruster.block.facing == dir)
                thruster.FireAttitude();
        }
    }
    
    void OnCollisionEnter(Collision collision) {
        var obj = collision.rigidbody.gameObject;
        
        if (collision.contacts.Length == 0) return;
        
        /*if (shields != null && collision.contacts[0].thisCollider.gameObject == shields.gameObject) {
            shields.OnCollisionEnter(collision);
            return;
        }*/
        
        if (obj.tag == "Item") {
            //scrapAvailable += 10;
            Pool.Recycle(obj);
            //foreach (var beam in GetBlockComponents<TractorBeam>()) {
            //if (beam.captured.Contains(obj.GetComponent<Collider>())) {
            //}
            //}
        }
        
        var otherForm = obj.GetComponent<Blockform>();
        if (otherForm != null) {
            foreach (var block in otherForm.BlocksAtWorldPos(collision.collider.transform.position)) {
                otherForm.ReceiveImpact(rigidBody, block);
            }
        }
    }
    
    void OnCollisionStay(Collision col) {
        if (BlocksAtWorldPos(col.contacts[0].point).Any())  {            
            var form = col.rigidbody.GetComponent<Blockform>();
            if (form == null) return;
            if (form.BlocksAtWorldPos(col.contacts[0].point).Any()) {                
                var awayDir = (col.rigidbody.transform.position - transform.position).normalized;
                col.rigidbody.MovePosition(col.rigidbody.transform.position + awayDir * length * 2);
            }
        }
    }
    
    void OnCollisionExit(Collision collision) {
        /*if (shields != null) {
            shields.OnCollisionExit(collision);
            return;
        }*/
    }
    
    public void StartTractorBeam(Vector2 pz) {
        foreach (var tractorBeam in GetBlockComponents<TractorBeam>()) {
            tractorBeam.Fire(pz);
        }
    }
    
    public void StopTractorBeam() {
        foreach (var tractorBeam in GetBlockComponents<TractorBeam>()) {
            tractorBeam.Stop();
        }
    }

	public void FoldJump() {
		Pool.Recycle(this.gameObject);
	}

	void Update() {
		AvoidCollision();
        var center = box.transform.position;
		Debug.DrawLine(center + -transform.right, center + transform.right, Color.green);
		Debug.DrawLine(center + transform.up, center + -transform.up, Color.green);
	}
}