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
    
    public bool hasGravity = false;
    
    public Shields shields = null;
    
    public Vector3 localCenter;
    
    public List<CrewBody> maglockedCrew = new List<CrewBody>();
    private bool needsMassUpdate = true;
    
	private HashSet<IntVector2> breaksToCheck = new HashSet<IntVector2>();

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

        foreach (var block in ship.blocks.AllBlocks) {
            OnBlockAdded(block);
        }

        foreach (var crew in ship.crew) {
            var body = Pool.For("CrewBody").Take<CrewBody>();
            var floor = Util.GetRandom(blocks.Find<Floor>().ToList());
            body.transform.parent = transform;
            body.transform.localPosition = BlockToLocalPos(floor);
            body.crew = crew;
            body.name = crew.name;
            body.gameObject.SetActive(true);
        }
    }

    void OnEnable() {
        Game.activeSector.blockforms.Add(this);
        InvokeRepeating("UpdateMass", 0.0f, 0.5f);
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

	public Orientation GetSideWithMostWeapons() {
		var sideCount = new Dictionary<Orientation, int>();
		sideCount[Orientation.up] = 0;
		foreach (var launcher in GetBlockComponents<TorpedoLauncher>()) {
			if (!sideCount.ContainsKey(launcher.block.orientation))
				sideCount[launcher.block.orientation] = 0;
			sideCount[launcher.block.orientation] += 1;
		}
		return sideCount.OrderBy((kv) => -kv.Value).First().Key;
	}
    
    public GameObject BreakBlock(Block block) {
        blocks[block.pos, block.layer] = null;
        
        /*var newShipObj = Pool.For("Ship").TakeObject();
        newShipObj.transform.position = BlockToWorldPos(block.pos);
        var newShip = newShipObj.GetComponent<Ship>();
        newShip.blocks[0, 0] = block;
        newShipObj.SetActive(true);
        newShip.rigidBody.velocity = rigidBody.velocity;
        newShip.rigidBody.angularVelocity = rigidBody.angularVelocity;*/
        //newShip.hasCollision = false;
        
        var obj = Pool.For("Item").TakeObject();
        obj.transform.position = BlockToWorldPos(block.pos);
        obj.SetActive(true);
        var rigid = obj.GetComponent<Rigidbody>();
        rigid.velocity = rigidBody.velocity;
        rigid.angularVelocity = rigidBody.angularVelocity;
        
        if (blocks.Count == 0) Pool.Recycle(gameObject);
        
        return obj;
    }

	public void BreakDetected(HashSet<Block> frag1, HashSet<Block> frag2) {
		var bigger = frag1.Count > frag2.Count ? frag1 : frag2;
		var smaller = frag1.Count > frag2.Count ? frag2 : frag1;

		foreach (var block in smaller) {
			foreach (var layerBlock in blocks[block.pos]) {
				BreakBlock(layerBlock);
			}
		}
	}

	public void CheckBreaks(IntVector2 breakPos) {
		var fills = new List<HashSet<Block>>();
		foreach (var neighbor in IntVector2.Neighbors(breakPos)) {
			var block = blocks[neighbor, BlockLayer.Base];
			if (block == null) continue;

			fills.Add(BlockPather.Floodfill(blocks, block));
		}

		while (fills.Count > 0) {
			var fill = fills.First();
			fills.Remove(fill);
			foreach (var other in fills) {
				if (!other.SetEquals(fill)) {
					BreakDetected(other, fill);
				}

			}
		}
	}
    
    public void OnBlockRemoved(Block oldBlock) {
        Profiler.BeginSample("OnBlockRemoved");
        
        //if (oldBlock.layer == BlockLayer.Base)
        //    this.size -= 1;
        
        UpdateBlock(oldBlock);
        
		CheckBreaks(oldBlock.pos);

        Profiler.EndSample();
    }
    
    public void OnBlockAdded(Block newBlock) {

        //if (newBlock.layer == BlockLayer.Base)
        //    this.size += 1;
        
        UpdateBlock(newBlock);
        
        if (newBlock.type.isComplexBlock) {
            AddBlockComponent(newBlock);
        }    
    }
    
    public void UpdateBlock(Block block) {
        if (block.mass != 0)
            needsMassUpdate = true;
        
        if (Block.Is<ShieldGenerator>(block))
            UpdateShields();
        
        if (Block.Is<InertiaStabilizer>(block))
            UpdateGravity();
    }
    
    public void AddBlockComponent(Block block) {
        Vector2 worldOrient = transform.TransformVector(Util.orientToCardinal[block.orientation]);
        
        var obj = Pool.For(block.type.gameObject).TakeObject();        
        obj.transform.parent = blockComponentHolder.transform;
        obj.transform.position = BlockToWorldPos(block);
        obj.transform.up = worldOrient;
		obj.transform.localScale *= Tile.worldSize;
        block.gameObject = obj;
        foreach (var comp in block.gameObject.GetComponents<BlockComponent>()) {
            comp.block = block;
        }
        
        obj.SetActive(true);
        
    }
    
    public void UpdateMass() {        
        if (!needsMassUpdate) return;
        
        var totalMass = 0.0f;
        var avgPos = new IntVector2(0, 0);
        
        foreach (var block in blocks.AllBlocks) {
            totalMass += block.mass;
            avgPos.x += block.pos.x;
            avgPos.y += block.pos.y;
        }
        
        rigidBody.mass = totalMass;
        
        if (blocks.Count > 0) {
            avgPos.x /= blocks.Count;
            avgPos.y /= blocks.Count;
        }
        localCenter = BlockToLocalPos(avgPos);
        rigidBody.centerOfMass = localCenter;
        
        needsMassUpdate = false;
    }
    
    public void UpdateShields() {
        if (blocks.Has<ShieldGenerator>() && shields == null) {
            var shieldObj = Pool.For("Shields").TakeObject();
            shields = shieldObj.GetComponent<Shields>();
            shieldObj.transform.parent = transform;
            shieldObj.transform.localPosition = localCenter;
            shieldObj.SetActive(true);
        } else if (!blocks.Has<ShieldGenerator>() && shields != null) {
            shields.gameObject.SetActive(false);
            shields = null;
        }
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
    
    public float RotateTowards(Vector2 worldPos, Orientation ori = Orientation.up) {
        var dir = (worldPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
        var currentAngle = transform.localEulerAngles.z;

		if (ori == Orientation.up) {
			angle -= 90;
		} else if (ori == Orientation.right) {
		} else if (ori == Orientation.left) {
			angle += 90;
		} else if (ori == Orientation.down) {
			angle += 180;
		}
        
        if (Math.Abs(360+angle - currentAngle) < Math.Abs(angle - currentAngle)) {
            angle = 360+angle;
        }
        
        if (angle > currentAngle + 15) {
            FireAttitudeThrusters(Orientation.right);
        } else if (angle < currentAngle - 15) {
            FireAttitudeThrusters(Orientation.left);
        }
        
		return angle-currentAngle;
    }
    
    public void MoveTowards(Vector3 worldPos) {
		var angle = Vector3.Angle(transform.TransformDirection(Vector2.up), (worldPos-transform.position).normalized);
		if (angle < 60) {
			FireThrusters(Orientation.down);
		}

		if (angle > 180 - 60) {
			FireThrusters(Orientation.up);
		}
        /*var localDir = transform.InverseTransformDirection((worldPos - (Vector2)transform.position).normalized);
        var orient = Util.cardinalToOrient[Util.Cardinalize(localDir)];
        FireThrusters((Orientation)(-(int)orient));*/
    }

	public void AvoidCollision() {
		var seen = new HashSet<Rigidbody>();

		foreach (var ori in (Orientation[])Enum.GetValues(typeof(Orientation))) {
			var cardinal = transform.TransformDirection(Util.orientToCardinal[ori]);
			var cols = Util.ShipCast(this, transform.position + cardinal*height, transform.position + cardinal*height + cardinal*Tile.worldSize);

			foreach (var col in cols) {
				if (seen.Contains(col.rigidbody)) continue;
				if (col.rigidbody.gameObject.GetComponentInParent<Blockform>() == null) continue;

				seen.Add(col.rigidbody);

				var local = transform.InverseTransformPoint(col.point);
				if (local.x > 0)
					FireThrusters(Orientation.right);
				if (local.x < 0)
					FireThrusters(Orientation.left);
				if (local.y > 0)
					FireThrusters(Orientation.up);
				if (local.y < 0)
					FireThrusters(Orientation.down);
			}
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
        return blocks[WorldToBlockPos(worldPos)];
    }
    
    public void FireThrusters(Orientation orientation) {
        foreach (var thruster in GetBlockComponents<Thruster>()) {
            if (thruster.block.orientation == orientation)
                thruster.Fire();
        }
    }
    
    public void FireAttitudeThrusters(Orientation orientation) {
        foreach (var thruster in GetBlockComponents<Thruster>()) {
            if (thruster.block.orientation == orientation)
                thruster.FireAttitude();
        }
    }
    
    void OnCollisionEnter(Collision collision) {
        var obj = collision.rigidbody.gameObject;
        
        if (collision.contacts.Length == 0) return;
        
        if (shields != null && collision.contacts[0].thisCollider.gameObject == shields.gameObject) {
            shields.OnCollisionEnter(collision);
            return;
        }
        
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
        if (shields != null) {
            shields.OnCollisionStay(collision);
            return;
        }
    }
    
    void OnCollisionExit(Collision collision) {
        if (shields != null) {
            shields.OnCollisionExit(collision);
            return;
        }
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

	void Update() {
		AvoidCollision();
	}

    void FixedUpdate() {
        if (transform.position.magnitude > Game.activeSector.sector.radius) {
            var towardsCenter = (Vector3.zero - transform.position).normalized;
            var factor = transform.position.magnitude - Game.activeSector.sector.radius;
            rigidBody.AddForce(towardsCenter * factor * 10 * Time.fixedDeltaTime);
        }
    }
}