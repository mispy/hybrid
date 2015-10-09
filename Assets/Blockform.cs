using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Blockform : PoolBehaviour {
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
    
    public Ship ship;
    
    public Blueprint blueprint;
    public BlockMap blocks;
    public TileRenderer tiles;
    public Rigidbody rigidBody;
	public BoxCollider boundsCollider;
    public ShipDamage damage;
    
    public bool hasGravity = false;
    
    public Shields shields = null;
    
    public Vector3 centerOfMass;
	public InteriorFog fog;

    
    public List<CrewBody> maglockedCrew = new List<CrewBody>();
    private bool needsMassUpdate = true;

    public GameObject blockComponentHolder;
	public SpacePather pather;

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

	public float radius {
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

	public Bounds bounds;

    public IEnumerable<T> GetBlockComponents<T>() {
        return GetComponentsInChildren<T>().Where((comp) => (comp as BlockComponent).block.ship == ship);
    }
    
    public bool HasBlockComponent<T>() {
        return GetBlockComponents<T>().ToList().Count > 0;
    }
    
    public void Initialize(Ship ship) {
        this.ship = ship;
        this.name = ship.name;
        this.blocks = ship.blocks;
        
        rigidBody = GetComponent<Rigidbody>();
        tiles = GetComponent<TileRenderer>();
        damage = GetComponent<ShipDamage>();
        blocks.OnBlockRemoved += OnBlockRemoved;
        blocks.OnBlockAdded += OnBlockAdded;
        
        var obj = Pool.For("Blueprint").TakeObject();
        obj.transform.parent = transform;
        obj.transform.position = transform.position;
        obj.SetActive(true);
        blueprint = obj.GetComponent<Blueprint>();
        blueprint.Initialize(ship);
		blueprint.tiles.DisableRendering();
        
        obj = Pool.For("Holder").TakeObject();
        obj.transform.parent = transform;
        obj.transform.position = transform.position;
        obj.name = "BlockComponents";
        obj.SetActive(true);
        blockComponentHolder = obj;

		obj = Pool.For("Holder").TakeObject();
		obj.transform.SetParent(transform);
		obj.transform.position = transform.position;
		obj.name = "SpacePather";
		pather = obj.AddComponent<SpacePather>();
		obj.SetActive(true);

		fog = Pool.For("InteriorFog").Take<InteriorFog>();
		fog.transform.SetParent(transform);
		fog.transform.position = transform.position;
		fog.name = "InteriorFog";
		fog.gameObject.SetActive(true);

		boundsCollider = Pool.For("BoundsCollider").Take<BoxCollider>();
		boundsCollider.isTrigger = true;
		boundsCollider.transform.SetParent(transform);
		boundsCollider.transform.position = transform.position;
		boundsCollider.gameObject.SetActive(true);

        foreach (var block in ship.blocks.allBlocks) {
            OnBlockAdded(block);
        }

        foreach (var crew in ship.crew) {
            var body = Pool.For("CrewBody").Take<CrewBody>();
            var floor = Util.GetRandom(blocks.Find("Floor").ToList());
            body.transform.parent = transform;
            body.transform.localPosition = BlockToLocalPos(floor);
            body.crew = crew;
            body.name = crew.name;
            body.gameObject.SetActive(true);
        }
    }

    void OnEnable() {
        Game.activeSector.blockforms.Add(this);
        InvokeRepeating("UpdateMass", 0f, 0.5f);
    }
    
    void OnDisable() {
        Game.activeSector.blockforms.Remove(this);
	}
    
    public void ReceiveImpact(Rigidbody fromRigid, Block block) {
        var impactVelocity = rigidBody.velocity - fromRigid.velocity;
        var impactForce = impactVelocity.magnitude * fromRigid.mass;
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


        Profiler.EndSample();
    }
    
    public void OnBlockAdded(Block newBlock) {

        //if (newBlock.layer == BlockLayer.Base)
        //    this.size += 1;
        
        UpdateBlock(newBlock);
        
        if (newBlock.type.isComplexBlock) {
            RealizeBlock(newBlock);
        }    
    }

	public void UpdateBounds() {
		bounds = new Bounds(blocks.boundingRect.center * Tile.worldSize, blocks.boundingRect.size * Tile.worldSize + new Vector2(Tile.worldSize, Tile.worldSize));
		boundsCollider.transform.localPosition = bounds.center;
		boundsCollider.size = bounds.size;
	}
    
    public void UpdateBlock(Block block) {
        if (block.mass != 0)
            needsMassUpdate = true;
        
		UpdateBounds();

        if (block.Is<InertiaStabilizer>())
            UpdateGravity();
    }
    
    public GameObject RealizeBlock(Block block) {
        Vector2 worldOrient = transform.TransformVector((Vector2)block.facing);
        
        var obj = Pool.For(block.type.gameObject).TakeObject();        
        block._gameObject = obj;
        obj.transform.SetParent(blockComponentHolder.transform);
        obj.transform.position = BlockToWorldPos(block);
        obj.transform.up = worldOrient;
        obj.transform.localScale *= Tile.worldSize;

        foreach (var comp in obj.GetComponents<BlockComponent>()) {
            comp.block = block;
        }

        if (!block.type.isComplexBlock)
            obj.GetComponent<SpriteRenderer>().enabled = false;

        obj.SetActive(true);
        return obj;
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
    
    public void UpdateGravity() {
        if (blocks.Has<InertiaStabilizer>() && hasGravity == false) {
            hasGravity = true;
            rigidBody.drag = 2;
            rigidBody.angularDrag = 2;
        } else if (!blocks.Has<InertiaStabilizer>() && hasGravity == true) {
            hasGravity = false;
            rigidBody.drag = 0;
            rigidBody.angularDrag = 0;
        }
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
		foreach (var form in Util.ShipsInRadius(transform.position, radius*2)) {
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
    
    void OnCollisionStay(Collision collision) {
        /*if (shields != null) {
            shields.OnCollisionStay(collision);
            return;
        }*/
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

		Debug.DrawLine(transform.TransformPoint(bounds.center + Vector3.left), transform.TransformPoint(bounds.center + Vector3.right), Color.green);
		Debug.DrawLine(transform.TransformPoint(bounds.center + Vector3.up), transform.TransformPoint(bounds.center + Vector3.down), Color.green);
	}

    void FixedUpdate() {
        if (transform.position.magnitude > Game.activeSector.sector.radius) {
            var towardsCenter = (Vector3.zero - transform.position).normalized;
            var factor = transform.position.magnitude - Game.activeSector.sector.radius;
            rigidBody.AddForce(towardsCenter * factor * 10 * Time.fixedDeltaTime);
        }
    }
}