using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlockMap {
	public int width;
	public int height;
	public int centerX;
	public int centerY;

	private Dictionary<IntVector2, Block> blockPositions;
	private Block[] blockSequence;
	public Vector3[] meshVertices;
	public Vector2[] meshUV;
	public int[] meshTriangles;
	
	public Ship ship;

	public BlockMap(Ship ship) {
		this.ship = ship;

		blockPositions = new Dictionary<IntVector2, Block>(); 
		blockSequence = new Block[1];
		meshVertices = new Vector3[1*4];
		meshUV = new Vector2[1*4];
		meshTriangles = new int[1*6];
	} 

	public IEnumerable<IntVector2> Neighbors(IntVector2 bp) {
		yield return new IntVector2(bp.x-1, bp.y);
		yield return new IntVector2(bp.x+1, bp.y);
		yield return new IntVector2(bp.x, bp.y-1);
		yield return new IntVector2(bp.x, bp.y+1);
	}
	
	public bool IsEdge(IntVector2 bp) {
		Profiler.BeginSample("IsEdge");

		var ret = false;
		var block = this[bp];
		if (block != null) {
			foreach (var neighbor in Neighbors(bp)) {
				var other = this[neighbor];
				if (other == null || other.collisionLayer != block.collisionLayer) {
					ret = true;
				}
			}
		}

		Profiler.EndSample();
		return ret;
	}

	public IEnumerable<Block> All {
		get {
			return blockPositions.Values;
		}
	}

	public int Count {
		get {
			return blockPositions.Count;
		}
	}

	public IEnumerable<Vector3> GetVertices(int x, int y) {
		var lx = x * Block.worldSize;
		var ly = y * Block.worldSize;
		yield return new Vector3(lx - Block.worldSize / 2, ly + Block.worldSize / 2, 0);
		yield return new Vector3(lx + Block.worldSize / 2, ly + Block.worldSize / 2, 0);
		yield return new Vector3(lx + Block.worldSize / 2, ly - Block.worldSize / 2, 0);
		yield return new Vector3(lx - Block.worldSize / 2, ly - Block.worldSize / 2, 0);
	/*meshVertices.Add(new Vector3(lx, ly, 0));
				meshVertices.Add(new Vector3(lx + Block.worldSize, ly, 0));
				meshVertices.Add(new Vector3(lx + Block.worldSize, ly - Block.worldSize, 0));
				meshVertices.Add(new Vector3(lx, ly - Block.worldSize, 0));*/
	
	}

	public void AttachToMesh(Block block) {		
		var i = block.index;
		blockSequence[i] = block;

		meshTriangles[i*6] = i*4;
		meshTriangles[i*6+1] = (i*4)+1;
		meshTriangles[i*6+2] = (i*4)+3;
		meshTriangles[i*6+3] = (i*4)+1;
		meshTriangles[i*6+4] = (i*4)+2;
		meshTriangles[i*6+5] = (i*4)+3;
		
		var verts = GetVertices(block.pos.x, block.pos.y).ToList();
		meshVertices[i*4] = verts[0];
		meshVertices[i*4+1] = verts[1];
		meshVertices[i*4+2] = verts[2];
		meshVertices[i*4+3] = verts[3];
		
		var uvs = Block.GetUVs(block);
		meshUV[i*4] = uvs[0];
		meshUV[i*4+1] = uvs[1];
		meshUV[i*4+2] = uvs[2];
		meshUV[i*4+3] = uvs[3];
	}

	public void ClearMeshPos(int i) {
		meshVertices[i*4] = Vector3.zero;
		meshVertices[i*4+1] = Vector3.zero;
		meshVertices[i*4+2] = Vector3.zero;
		meshVertices[i*4+3] = Vector3.zero;
	}

	public void ExpandBlockSequence(int newSize=-1) {
		if (newSize == -1)
			newSize = blockSequence.Length << 1;

		Debug.LogFormat("Expanding to {0}", newSize);

		var newSequence = new Block[newSize];
		var newTriangles = new int[newSize*6];
		var newVertices = new Vector3[newSize*4];
		var newUV = new Vector2[newSize*4];

		for (var i = 0; i < blockSequence.Length; i++) {
			newSequence[i] = blockSequence[i];
		}

		for (var i = 0; i < meshTriangles.Length; i++) {
			newTriangles[i] = meshTriangles[i];
		}

		for (var i = 0; i < meshVertices.Length; i++) {
			newVertices[i] = meshVertices[i];
			newUV[i] = meshUV[i];
		}
	
		blockSequence = newSequence;
		meshTriangles = newTriangles;
		meshVertices = newVertices;
		meshUV = newUV;
	}

	public Block this[IntVector2 bp] {
		get {
			Profiler.BeginSample("blocks[]");
			Block ret = null;
			blockPositions.TryGetValue(bp, out ret);
			Profiler.EndSample();
			return ret;
		}
		set {
			var currentBlock = this[bp];

			if (value != null) {
				value.pos = bp;
				value.ship = ship;				
				blockPositions[bp] = value; 
			} else {
				blockPositions.Remove(bp);
			}


			if (value == null && currentBlock == null) {
				return;
			} else if (value == null && currentBlock != null) {
				// removing an existing block
				ClearMeshPos(currentBlock.index);
				ship.OnBlockChanged(value, currentBlock);
			} else if (value != null && currentBlock == null) {
				// adding a new block
				while (true) {
					for (var i = 0; i < blockSequence.Length; i++) {
						if (blockSequence[i] == null) {
							value.index = i;
							AttachToMesh(value);
							ship.OnBlockChanged(value, currentBlock);					
							return;
						}
					}

					ExpandBlockSequence();
				}
			} else if (value != null && currentBlock != null) {
				// replacing an existing block
				value.index = currentBlock.index;
				AttachToMesh(value);
				ship.OnBlockChanged(value, currentBlock);
			}

		}
	}

	public Block this[int x, int y] {
		get { return this[new IntVector2(x, y)]; }
		set { this[new IntVector2(x, y)] = value; }
	}
}

public class Ship : MonoBehaviour {
	public BlockMap blocks;

	public Rigidbody rigidBody;

	
	private Mesh mesh;

	public bool hasCollision = true;
	public bool hasGravity = false;

	public Dictionary<IntVector2, GameObject> colliders = new Dictionary<IntVector2, GameObject>();

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap(this);
		rigidBody = GetComponent<Rigidbody>();
		mesh = GetComponent<MeshFilter>().mesh;		
	}

	void Start() {
		UpdateBlocks();

		if (hasCollision) {
			foreach (var block in blocks.All) {
				if (blocks.IsEdge(block.pos)) {
					AddCollider(block);
				}
			}
		}

		InvokeRepeating("UpdateMesh", 0.0f, 0.05f);
	}

	public void SetBlock(int x, int y, int type) {
		var block = new Block(type);
		blocks[x, y] = block;
	}

	public void SetBlock(int x, int y, int type, Vector2 orientation) {
		var block = new Block(type);
		block.orientation = orientation;
		blocks[x, y] = block;
	}

	public void ReceiveImpact(Rigidbody fromRigid, Block block) {
		// no point breaking off a single block from itself
		/*if (blocks.Count == 1) return;
		var myRigid = GetComponent<Rigidbody2D>();

		var impactVelocity = myRigid.velocity - fromRigid.velocity;
		var impactForce = impactVelocity.magnitude * fromRigid.mass;
		if (impactForce < 5) return;

		// break it off into a separate fragment
		var newShip = Instantiate(Game.main.shipPrefab, block.transform.position, block.transform.rotation) as GameObject;
		var newRigid = newShip.GetComponent<Rigidbody2D>();
		newRigid.velocity = myRigid.velocity;
		newRigid.angularVelocity = myRigid.angularVelocity;
		
		var newShipScript = newShip.GetComponent<Ship>();
		RemoveBlock(block);
		newShipScript.AddBlock(block, 0, 0);*/
	}

	public Ship BreakBlock(Block block) {
		if (blocks.Count == 0) // no use breaking a single block into itself
			return this;

		blocks[block.pos] = null;
		return this;

		var newShipObj = Pool.Ship.TakeObject();
		newShipObj.transform.position = BlockToWorldPos(block.pos);
		var newShip = newShipObj.GetComponent<Ship>();
		newShip.blocks[0, 0] = block;
		newShip.UpdateBlocks();
		var newRigid = newShipObj.GetComponent<Rigidbody>();
		newRigid.velocity = rigidBody.velocity;
		newRigid.angularVelocity = rigidBody.angularVelocity;
		newShipObj.SetActive(true);

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
		if (gameObject.activeInHierarchy) {			
			QueueMeshUpdate();

			var pos = newBlock == null ? oldBlock.pos : newBlock.pos;
			
			if (hasCollision) {
				foreach (var other in blocks.Neighbors(pos)) {
					UpdateCollider(other);
				}
				
				UpdateCollider(pos);
			}
		}
		Profiler.EndSample();
	}

	public void UpdateBlocks() {
		Profiler.BeginSample("UpdateBlocks");

		QueueMeshUpdate();

		var mass = 0.0f;
		
		foreach (var block in blocks.All) {
			if (block.type == Block.types["console"]) {
				hasGravity = true;
			}
			
			mass += Block.mass;
		}
		
		rigidBody.mass = mass;

		Profiler.EndSample();
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		var localPos = transform.InverseTransformPoint(worldPos);
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

	public Block BlockAtWorldPos(Vector2 worldPos) {
		return blocks[WorldToBlockPos(worldPos)];
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
				rigidBody.AddForceAtPosition(worldOrient * 0.005f, BlockToWorldPos(block.pos));
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

				/*var awayDir = newShip.transform.position - ship.transform.position;
				awayDir.Normalize();
				// make the block fly away from the ship
				newShip.rigidBody.AddForce(awayDir * Block.mass * 10);*/

				//var towardDir = newShip.transform.position - beam.transform.position;
				//towardDir.Normalize();
				//newShip.rigidBody.AddForce(towardDir * Block.mass * 100);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.transform.parent == null)
			return;

		var otherShip = collider.gameObject.transform.parent.GetComponent<Ship>();
		otherShip.ReceiveImpact(rigidBody, collider.gameObject.GetComponent<Block>());
	}


	void OnCollisionEnter(Collision collision) {
	}

	private bool needMeshUpdate = false;

	void QueueMeshUpdate() {
		needMeshUpdate = true;
	}

	void UpdateMesh() {
		Profiler.BeginSample("UpdateMesh");

		if (!needMeshUpdate) return;
		//mesh.Clear();
		mesh.vertices = blocks.meshVertices;
		mesh.triangles = blocks.meshTriangles;
		mesh.uv = blocks.meshUV;
		//mesh.Optimize();
		//mesh.RecalculateNormals();	
		needMeshUpdate = false;

		Profiler.EndSample();
	}
}
