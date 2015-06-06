using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Ship : MonoBehaviour {
	public static GameObject prefab;
	public static List<Ship> allActive = new List<Ship>();

	public BlockMap blocks;
	public Blueprint blueprint;
	
	public Rigidbody rigidBody;
	public MeshRenderer renderer;
	
	private Mesh mesh;
	
	public bool hasCollision = true;
	public bool hasGravity = false;
	
	public Dictionary<IntVector2, GameObject> colliders = new Dictionary<IntVector2, GameObject>();
	public Shields shields = null;
	
	public Vector3 localCenter;

	public static IEnumerable<Ship> ClosestTo(Vector2 worldPos) {
		return Ship.allActive.OrderBy((ship) => Vector2.Distance(ship.transform.position, worldPos));
	}

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
		blocks.OnBlockChanged = OnBlockChanged;
		rigidBody = GetComponent<Rigidbody>();
		renderer = GetComponent<MeshRenderer>();		
		mesh = GetComponent<MeshFilter>().mesh;	
	}
	
	void Start() {		
		if (hasCollision) {
			foreach (var block in blocks.All) {
				if (blocks.IsEdge(block.pos)) {
					AddCollider(block);
				}
			}
		}

				
		UpdateMass();	
		UpdateShields();	
		UpdateGravity();

		QueueMeshUpdate();
		InvokeRepeating("UpdateMesh", 0.0f, 0.05f);

		Ship.allActive.Add(this);
	}

	void Update() {
	}

	void OnDisable() {
		Ship.allActive.Remove(this);
	}

	public void SetBlock(int x, int y, int type) {
		var block = new Block(type);
		blocks[x, y] = block;
		var block2 = new Block(type);
		blueprint.blocks[x, y] = block2;
	}

	public void SetBlock(int x, int y, int type, Vector2 orientation) {
		var block = new Block(type);
		block.orientation = orientation;
		blocks[x, y] = block;

		var block2 = new Block(type);
		block2.orientation = orientation;
		blueprint.blocks[x, y] = block2;
	}

	public void ReceiveImpact(Rigidbody fromRigid, Block block) {
		var impactVelocity = rigidBody.velocity - fromRigid.velocity;
		var impactForce = impactVelocity.magnitude * fromRigid.mass;
		//if (impactForce < 5) return;

		// break it off into a separate fragment
		//BreakBlock(block);
	}

	public Ship BreakBlock(Block block) {
		if (blocks.Count == 1) // no use breaking a single block into itself
			return this;

		blocks[block.pos] = null;

		var newShipObj = Pool.ship.TakeObject();
		newShipObj.transform.position = BlockToWorldPos(block.pos);
		var newShip = newShipObj.GetComponent<Ship>();
		newShip.blocks[0, 0] = block;
		newShipObj.SetActive(true);
		newShip.rigidBody.velocity = rigidBody.velocity;
		newShip.rigidBody.angularVelocity = rigidBody.angularVelocity;
		//newShip.hasCollision = false;

		return newShip;
	}

	public void AddCollider(Block block) {
		Profiler.BeginSample("AddCollider");

		GameObject colliderObj;
		if (block.collisionLayer == Block.wallLayer)
			colliderObj = Pool.WallCollider.TakeObject();
		else
			colliderObj = Pool.FloorCollider.TakeObject();
		colliderObj.transform.parent = transform;
		colliderObj.transform.localPosition = BlockToLocalPos(block.pos);
		colliders[block.pos] = colliderObj;
		colliderObj.SetActive(true);

		Profiler.EndSample();
	}

	public void UpdateCollider(IntVector2 pos) {
		Profiler.BeginSample("UpdateCollider");

		var block = blocks[pos];
		var hasCollider = colliders.ContainsKey(pos);
		var isEdge = blocks.IsEdge(pos);

		if (hasCollider && (!isEdge || colliders[pos].layer != block.collisionLayer)) {
			colliders[pos].SetActive(false);
			colliders.Remove(pos);
			hasCollider = false;
		}

		if (!hasCollider && isEdge) {
			AddCollider(block);
		}

		Profiler.EndSample();
	}


	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		Profiler.BeginSample("OnBlockChanged");
		if (newBlock != null) newBlock.ship = this;

		// Inactive ships do not automatically update on block change, to allow
		// for performant pre-runtime mass construction. kinda like turning the power
		// off so you can stick your hand in there
		// - mispy
		if (!gameObject.activeInHierarchy) return;
		var pos = newBlock == null ? oldBlock.pos : newBlock.pos;

		UpdateCollision(pos);

		var oldMass = oldBlock == null ? 0 : oldBlock.mass;
		var newMass = newBlock == null ? 0 : newBlock.mass;
		if (oldMass != newMass)
			UpdateMass();

		if (Block.IsType(newBlock, "shieldgen") || Block.IsType(oldBlock, "shieldgen"))
			UpdateShields();

		if (Block.IsType(newBlock, "gravgen") || Block.IsType(oldBlock, "gravgen"))
			UpdateGravity();


		QueueMeshUpdate();		

		Profiler.EndSample();
	}
	
	public void UpdateCollision(IntVector2 pos) {
		if (!hasCollision) return;
		
		foreach (var other in blocks.Neighbors(pos)) {
			UpdateCollider(other);
		}
		
		UpdateCollider(pos);
	}

	public void UpdateMass() {		
		var totalMass = 0.0f;
		var avgPos = new IntVector2(0, 0);
		
		foreach (var block in blocks.All) {
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
	}

	public void UpdateShields() {
		if (blocks.HasType("shieldgen") && shields == null) {
			var shieldObj = Pool.shields.TakeObject();
			shields = shieldObj.GetComponent<Shields>();
			shieldObj.transform.parent = transform;
			shieldObj.transform.localPosition = localCenter;
			shieldObj.SetActive(true);
		} else if (!blocks.HasType("shieldgen") && shields != null) {
			shields.gameObject.SetActive(false);
			shields = null;
		}
	}

	public void UpdateGravity() {
		if (blocks.HasType("gravgen") && hasGravity == false) {
			hasGravity = true;
			rigidBody.drag = 5;
			rigidBody.angularDrag = 5;
		} else if (!blocks.HasType("gravgen") && hasGravity == true) {
			hasGravity = false;
			rigidBody.drag = 0;
			rigidBody.angularDrag = 0;
		}
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		return LocalToBlockPos(transform.InverseTransformPoint(worldPos));
	}

	public IntVector2 LocalToBlockPos(Vector3 localPos) {
		// remember that blocks go around the center point of the center block at [0,0]		
		return new IntVector2(Mathf.FloorToInt((localPos.x + Block.worldSize/2.0f) / Block.worldSize),
		                      Mathf.FloorToInt((localPos.y + Block.worldSize/2.0f) / Block.worldSize));
	}


	public Vector2 BlockToLocalPos(IntVector2 blockPos) {
		return new Vector2(blockPos.x*Block.worldSize, blockPos.y*Block.worldSize);
	}

	public Vector2 BlockToWorldPos(IntVector2 blockPos) {
		return transform.TransformPoint(BlockToLocalPos(blockPos));
	}

	public Block BlockAtLocalPos(Vector3 localPos) {
		return blocks[LocalToBlockPos(localPos)];
	}

	public Block BlockAtWorldPos(Vector2 worldPos) {
		Profiler.BeginSample("BlockAtWorldPos");
		var block = blocks[WorldToBlockPos(worldPos)];
		Profiler.EndSample();
		return block;
	}

	public Dictionary<IntVector2, ParticleSystem> particleCache = new Dictionary<IntVector2, ParticleSystem>();

	public void FireThrusters(Vector2 orientation) {
		foreach (var block in blocks.All) {
			if (block.type != Block.types["thruster"]) continue;

			if (block.orientation == orientation) {

				// need to flip thrusters on the vertical axis so they point the right way
				Vector2 worldOrient;
				if (block.orientation == Vector2.up || block.orientation == -Vector2.up) {
					worldOrient = transform.TransformVector(block.orientation);
				} else {
					worldOrient = transform.TransformVector(-block.orientation);
				}

				if (!particleCache.ContainsKey(block.pos)) {
					var ps = Pool.ParticleThrust.TakeObject().GetComponent<ParticleSystem>();
					ps.gameObject.SetActive(true);
					ps.transform.parent = transform;
					ps.transform.localPosition = BlockToLocalPos(block.pos);
					ps.transform.up = worldOrient;
					particleCache[block.pos] = ps;
				}
				var thrust = particleCache[block.pos];
				thrust.Emit(1);
				rigidBody.AddForce(worldOrient * Math.Min(rigidBody.mass * 10, Block.defaultMass * 1000));
			}
		}
	}

	private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[16];
	public void OnParticleCollision(GameObject psObj) {
		var ps = psObj.GetComponent<ParticleSystem>();
		var safeLength = ps.GetSafeCollisionEventSize();

		if (collisionEvents.Length < safeLength) {
			collisionEvents = new ParticleCollisionEvent[safeLength];
		}

		// get collision events for the gameObject that the script is attached to
		var numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);

		for (var i = 0; i < numCollisionEvents; i++) {
			//Debug.Log(collisionEvents[i].intersection);
			var pos = collisionEvents[i].intersection;
			var block = BlockAtWorldPos(pos);
			if (block != null) {
				BreakBlock(block);
			}
		}
	}

	public void FireLasers() {
		foreach (var block in blocks.All) {
			if (block.type != Block.types["laser"]) continue;

			Vector2 worldOrient;
			if (block.orientation == Vector2.up || block.orientation == -Vector2.up) {
				worldOrient = transform.TransformDirection(-block.orientation);
			} else {
				worldOrient = transform.TransformDirection(block.orientation);
			}

			ParticleSystem beam;
			if (particleCache.ContainsKey(block.pos)) {
				beam = particleCache[block.pos];
			} else {
				beam = Pool.ParticleBeam.TakeObject().GetComponent<ParticleSystem>();
				beam.transform.parent = transform;
				beam.transform.position = BlockToWorldPos(block.pos) + (worldOrient * Block.worldSize);
				beam.transform.up = worldOrient;
				particleCache[block.pos] = beam;
				beam.gameObject.SetActive(true);
			}
					
			//beam.enableEmission = true;
			beam.Emit(1);

			var hitBlocks = Block.FromHits(Util.ParticleCast(beam));
			foreach (var hitBlock in hitBlocks) {
				var ship = hitBlock.ship;
				if (ship == this) continue;
				var newShip = ship.BreakBlock(hitBlock);

				var awayDir = newShip.transform.position - ship.transform.position;
				awayDir.Normalize();
				// make the block fly away from the ship
				newShip.rigidBody.AddForce(awayDir * Block.defaultMass * 1000);

				//var towardDir = newShip.transform.position - beam.transform.position;
				//towardDir.Normalize();
				//newShip.rigidBody.AddForce(towardDir * Block.mass * 100);
			}
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (shields != null) {
			shields.OnCollisionEnter(collision);
			return;
		}

		var otherShip = collision.rigidbody.gameObject.GetComponent<Ship>();
		if (otherShip != null) {
			var block = otherShip.BlockAtWorldPos(collision.collider.transform.position);
			if (block != null)
				otherShip.ReceiveImpact(rigidBody, block);
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


	private bool needMeshUpdate = false;

	void QueueMeshUpdate() {
		needMeshUpdate = true;
	}

	void UpdateMesh() {
		Profiler.BeginSample("UpdateMesh");

		if (!needMeshUpdate) return;
		mesh.Clear();
		mesh.vertices = blocks.meshVertices;
		mesh.triangles = blocks.meshTriangles;
		mesh.uv = blocks.meshUV;
		mesh.Optimize();
		mesh.RecalculateNormals();	
		
		if (shields != null) {
			var hypo = Mathf.Sqrt(mesh.bounds.size.x*mesh.bounds.size.x + mesh.bounds.size.y*mesh.bounds.size.y);
			var scale = new Vector3(mesh.bounds.size.x, mesh.bounds.size.y, 1);
			scale.x += hypo * mesh.bounds.size.x / (mesh.bounds.size.x+mesh.bounds.size.y);
			scale.y += hypo * mesh.bounds.size.y / (mesh.bounds.size.x+mesh.bounds.size.y);

			shields.transform.localScale = scale;
		}
		needMeshUpdate = false;

		Profiler.EndSample();
	}
}
