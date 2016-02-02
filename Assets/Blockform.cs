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
                template.Fill(form);
                PrefabUtility.CreatePrefab(path, template.gameObject);
                Pool.Recycle(template.gameObject);
            } else {
                var template = (prefab as GameObject).GetComponent<ShipTemplate2>();
                template.Fill(form);
                PrefabUtility.ReplacePrefab(template.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                Pool.Recycle(template.gameObject);
            }

        }


        DrawDefaultInspector();
    }
}
#endif

public class Blockform : PoolBehaviour, ISaveable {
    [ReadOnlyAttribute]
    public Bounds localBounds = new Bounds();
    [ReadOnlyAttribute]
    public Vector3 centerOfMass;
    [ReadOnlyAttribute]
    public List<CrewBody> maglockedCrew = new List<CrewBody>();   
    [ReadOnlyAttribute]
    public HashSet<Block> poweredWeapons = new HashSet<Block>();

    public IEnumerable<CrewBody> friendlyCrew {
        get {
            return maglockedCrew;
        }
    }

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

    private int blockGUIDCounter = 0;
    private bool needsMassUpdate = true;

    public bool hasMindPilot {
        get {
            foreach (var console in GetBlockComponents<Console>()) {
                if (console.canAccessThrusters && console.crew != null && console.crew.mind != null)
                    return true;
            }
            return false;
        }
    }

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
        var ship = Pool.For("Blockform").Attach<Blockform>(Game.activeSector.contents);
        template.blocks.ReadBlockData();

        Debug.Assert(template.blocks.allBlocks.Count > 0, "Expected template.blocks.allBlocks.Count > 0 for " + template.name);
        ship.name = template.name;
        ship.Initialize(template);

        ship.gameObject.SetActive(true);
        return ship;
    }

    public struct BlockUpdate {
        public IntVector3 blockPos;
        public Block block;

        public BlockUpdate(IntVector3 blockPos, Block block)  {
            this.blockPos = blockPos;
            this.block = block;
        }
    }

    public List<BlockUpdate> blockUpdates = new List<BlockUpdate>();

    public override void OnSerialize(MispyNetworkWriter binary, bool initial) {               
        if (initial) {
            binary.Write(blocks.allBlocks.Count);
            foreach (var block in blocks.allBlocks) {
                binary.Write(block);
            }                  
        }

        if (!initial) {
            binary.Write(blockUpdates.Count);
            foreach (var update in blockUpdates) {
                binary.Write(update.blockPos);
                binary.Write(update.block);
            }
            blockUpdates.Clear();
        }
    }

    public override void OnDeserialize(MispyNetworkReader binary, bool initial) {
        if (initial) {
            var count = binary.ReadInt32();
            for (var i = 0; i < count; i++) {
                var block = binary.ReadBlock();
                blocks[block.pos, block.layer] = block;
            }        
        }

        if (!initial) {
            var count = binary.ReadInt32();
            for (var i = 0; i < count; i++) {
                var blockPos = binary.ReadIntVector3();
                var block = binary.ReadBlock();
                blocks[blockPos] = block;
            }
        }
    }

    void ISaveable.OnSave(SaveWriter save) {
        save.Write(blocks.allBlocks.Count);
        foreach (var block in blocks.allBlocks) {
            save.Write(block);
        }                 
    }

    void ISaveable.OnLoad(SaveReader save) {
        var count = save.ReadInt32();
        for (var i = 0; i < count; i++) {
            var block = save.ReadBlock();
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

    public void AddCrew() {
        var floor = Util.GetRandom(blocks.Find("Floor").ToList());
        var crew = Pool.For("Crew").Attach<CrewBody>(transform);
        crew.transform.position = BlockToWorldPos(floor);
    }

    public void Awake() {
        channel = Channel.ReliableFragmented;
        rigidBody = GetComponent<Rigidbody>();
        tiles = GetComponent<TileRenderer>();
        damage = GetComponent<ShipDamage>();
        pather = GetComponent<SpacePather>();
        box = Pool.For("BoundsCollider").Attach<BoxCollider>(transform);
        blockCompCache = new Dictionary<Type, HashSet<BlockComponent>>();
        box.isTrigger = true;
        blockComponentHolder = Pool.For("Holder").Attach<Transform>(transform);
        blockComponentHolder.name = "BlockComponents";
        blocks = Pool.For("BlockMap").Attach<BlockMap>(transform);
        blocks.OnBlockAdded += OnBlockAdded;
        blocks.OnBlockRemoved += OnBlockRemoved;
        blocks.ship = this;

        if (SpaceNetwork.isServer) {
            guid = GUID.Assign();

            // Reserve a bunch of guids for our block components so we don't
            // have to negotiate each of them
            GUID.Reserve(10000);
        }
    }

    public void Initialize(ShipTemplate2 template) {
        //blueprint = Pool.For("Blueprint").Attach<Blueprint>(transform);
        //blueprint.Initialize();
        //blueprint.tiles.DisableRendering();

        foreach (var block in template.blocks.allBlocks) {
            blocks[block.pos, block.layer] = new Block(block);
            //blueprint.blocks[block.pos, block.layer] = new Block(block);
        }

    }

    public bool hasStarted = false;

    void Start() {
        //Pool.For("InteriorFog").Attach<InteriorFog>(transform);

        Debug.Assert(blocks.allBlocks.Count() > 0, "Expected allBlocks.Count() > 0");
        foreach (var block in blocks.allBlocks) {
            OnBlockAdded(block);
        }
        
        Game.activeSector.blockforms.Add(this);
        InvokeRepeating("UpdateMass", 0f, 0.5f);
        hasStarted = true;

        AddCrew();
        AddCrew();

/*        foreach (var block in Game.playerShip.blocks.allBlocks) {
            block.health /= 2.0f;
        }*/
    }
    
    void OnDisable() {
        Game.activeSector.blockforms.Remove(this);
	}

    void OnEnable() {
        Game.activeSector.blockforms.Add(this);
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
            if (!launcher.charger.isReady)
                continue;
            
			if (!sideCount.ContainsKey(launcher.block.facing))
				sideCount[launcher.block.facing] = 0;
			sideCount[launcher.block.facing] += 1;
		}
		return sideCount.OrderBy((kv) => -kv.Value).First().Key;
	}


    public void OnBlockRemoved(Block oldBlock) {
        Profiler.BeginSample("OnBlockRemoved");

        UpdateBlock(oldBlock);       

        if (oldBlock.gameObject != null) {
            foreach (var comp in oldBlock.gameObject.GetComponents<BlockComponent>()) {
                if (blockCompCache.ContainsKey(comp.GetType()))
                    blockCompCache[comp.GetType()].Remove(comp);
            }

            Pool.Recycle(oldBlock.gameObject);
        }
          
        if (hasStarted && !deserializing && blocks[oldBlock.blockPos] == null) {
            blockUpdates.Add(new BlockUpdate(oldBlock.blockPos, null));
            SpaceNetwork.Sync(this);
        }
        Profiler.EndSample();
    }

    public void OnBlockAdded(Block newBlock) {
        var isExisting = newBlock.ship == this;
            
        newBlock.ship = this;
        UpdateBlock(newBlock);
        
        if (newBlock.type.isComplexBlock || blocks.IsCollisionEdge(newBlock)) {
            RealizeBlock(newBlock);
        }    

        if (hasStarted && !deserializing && !isExisting) {
            blockUpdates.Add(new BlockUpdate(newBlock.blockPos, newBlock));
            SpaceNetwork.Sync(this);
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
        if (block.gameObject != null) return;

        Vector2 worldOrient = transform.TransformVector((Vector2)block.facing);

        var obj = Pool.For(block.type.gameObject).Attach<Transform>(blockComponentHolder, false);

        obj.transform.SetParent(blockComponentHolder);
        obj.transform.position = BlockToWorldPos(block);
        obj.transform.up = worldOrient;
        obj.transform.localScale *= Tile.worldSize;
        
        block.gameObject = obj.gameObject;
        
        foreach (var comp in obj.GetComponents<BlockComponent>()) {
            comp.block = block;
            comp.form = this;
            comp.OnRealize();

            if (!blockCompCache.ContainsKey(comp.GetType()))
                blockCompCache[comp.GetType()] = new HashSet<BlockComponent>();
            blockCompCache[comp.GetType()].Add(comp);

            comp.guid = new GUID(guid.value + (blockGUIDCounter += 1));
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
        DebugUtil.DrawPoint(transform, box.transform.position);
	}
}