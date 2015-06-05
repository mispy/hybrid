using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlockMap {
	private Dictionary<IntVector2, Block> blockPositions;
	private Block[] blockSequence;
	public Vector3[] meshVertices;
	public Vector2[] meshUV;
	public int[] meshTriangles;

	public int maxX;
	public int minX;
	public int maxY;
	public int minY;

	public Ship ship;

	public Dictionary<int, List<Block>> blockTypeCache;

	public BlockMap(Ship ship) {
		this.ship = ship;

		blockPositions = new Dictionary<IntVector2, Block>(); 
		blockSequence = new Block[1];
		meshVertices = new Vector3[1*4];
		meshUV = new Vector2[1*4];
		meshTriangles = new int[1*6];

		minX = 0;
		minY = 0;
		maxX = 0;
		maxY = 0;

		blockTypeCache = new Dictionary<int, List<Block>>();
		foreach (var type in Block.types.Values) {
			blockTypeCache[type] = new List<Block>();
		}
	} 

	public IntVector2[] Neighbors(IntVector2 bp) {
		return new IntVector2[] {
			new IntVector2(bp.x-1, bp.y),
			new IntVector2(bp.x+1, bp.y),
	 		new IntVector2(bp.x, bp.y-1),
			new IntVector2(bp.x, bp.y+1)
		};
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

		//Debug.LogFormat("Expanding to {0}", newSize);

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

	public Block FindType(int type) {
		foreach (var block in blockTypeCache[type]) {
			return block;
		}

		return null;
	}

	public bool HasType(int type) {
		return blockTypeCache[type].Count > 0;
	}

	public bool HasType(string typeName) {
		return blockTypeCache[Block.types[typeName]].Count > 0;
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

				if (bp.x > maxX)
					maxX = bp.x;
				if (bp.y > maxY)
					maxY = bp.y;
				if (bp.x < minX)
					minX = bp.x;
				if (bp.y < minY)
					minY = bp.y;
			} else {
				blockPositions.Remove(bp);
			}


			if (value == null && currentBlock == null) {
				return;
			} else if (value == null && currentBlock != null) {
				// removing an existing block
				ClearMeshPos(currentBlock.index);
				blockTypeCache[currentBlock.type].Remove(currentBlock);
				ship.OnBlockChanged(value, currentBlock);
			} else if (value != null && currentBlock == null) {
				// adding a new block
				while (true) {
					for (var i = 0; i < blockSequence.Length; i++) {
						if (blockSequence[i] == null) {
							value.index = i;
							AttachToMesh(value);
							blockTypeCache[value.type].Add(value);
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
				blockTypeCache[currentBlock.type].Remove(currentBlock);
				blockTypeCache[value.type].Add(value);
				ship.OnBlockChanged(value, currentBlock);
			}

		}
	}

	public Block this[int x, int y] {
		get { return this[new IntVector2(x, y)]; }
		set { this[new IntVector2(x, y)] = value; }
	}

	public bool IsPassable(IntVector2 bp) {
		return (this[bp] == null || this[bp].collisionLayer == Block.floorLayer);
	}

	public List<IntVector2> PathBetween(IntVector2 start, IntVector2 end) {
		//Debug.LogFormat("{0} {1} {2} {3}", minX, minY, maxX, maxY);
		// nodes that have already been analyzed and have a path from the start to them
		var closedSet = new List<IntVector2>();
		// nodes that have been identified as a neighbor of an analyzed node, but have 
		// yet to be fully analyzed
		var openSet = new List<IntVector2> { start };
		// a dictionary identifying the optimal origin Cell to each node. this is used 
		// to back-track from the end to find the optimal path
		var cameFrom = new Dictionary<IntVector2, IntVector2>();
		// a dictionary indicating how far each analyzed node is from the start
		var currentDistance = new Dictionary<IntVector2, int>();
		// a dictionary indicating how far it is expected to reach the end, if the path 
		// travels through the specified node. 
		var predictedDistance = new Dictionary<IntVector2, float>();
		
		// initialize the start node as having a distance of 0, and an estmated distance 
		// of y-distance + x-distance, which is the optimal path in a square grid that 
		// doesn't allow for diagonal movement
		currentDistance.Add(start, 0);
		predictedDistance.Add(
			start,
			0 + +Math.Abs(start.x - end.x) + Math.Abs(start.x - end.x)
			);
		
		// if there are any unanalyzed nodes, process them
		while (openSet.Count > 0) {
			// get the node with the lowest estimated cost to finish
			
			var current = (
				from p in openSet orderby predictedDistance[p] ascending select p
				).First();
			
			// if it is the finish, return the path
			if (current.x == end.x && current.y == end.y) {
				// generate the found path
				return ReconstructPath(cameFrom, end);
			}
			
			// move current node from open to closed
			openSet.Remove(current);
			closedSet.Add(current);
			
			// process each valid node around the current node
			foreach (var neighbor in Neighbors(current)) {
				if (neighbor.x > maxX+2 || neighbor.x < minX-2 || neighbor.y > maxY+2 || neighbor.y < minY-2 || !IsPassable(neighbor)) {
					continue;
				}
				
				var tempCurrentDistance = currentDistance[current] + 1;
				
				// if we already know a faster way to this neighbor, use that route and 
				// ignore this one
				if (closedSet.Contains(neighbor)
				    && tempCurrentDistance >= currentDistance[neighbor]) {
					continue;
				}
				
				// if we don't know a route to this neighbor, or if this is faster, 
				// store this route
				if (!closedSet.Contains(neighbor)
				    || tempCurrentDistance < currentDistance[neighbor]) {
					if (cameFrom.Keys.Contains(neighbor)) {
						cameFrom[neighbor] = current;
					}
					else {
						cameFrom.Add(neighbor, current);
					}
					
					currentDistance[neighbor] = tempCurrentDistance;
					predictedDistance[neighbor] =
						currentDistance[neighbor]
						+ Math.Abs(neighbor.x - end.x)
							+ Math.Abs(neighbor.y - end.y);
					
					// if this is a new node, add it to processing
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					}
				}
			}
		}
		
		// unable to figure out a path, abort.
		return null;
	}

	/// <summary>
	/// Process a list of valid paths generated by the Pathfind function and return 
	/// a coherent path to current.
	/// </summary>
	/// <param name="cameFrom">A list of nodes and the origin to that node.</param>
	/// <param name="current">The destination node being sought out.</param>
	/// <returns>The shortest path from the start to the destination node.</returns>
	public List<IntVector2> ReconstructPath(Dictionary<IntVector2, IntVector2> cameFrom, IntVector2 current) {
		if (!cameFrom.Keys.Contains(current)) {
			return new List<IntVector2> { current };
		}
		
		var path = ReconstructPath(cameFrom, cameFrom[current]);
		path.Add(current);
		return path;
	}
}

public class Ship : MonoBehaviour {
	public static List<Ship> allActive = new List<Ship>();

	public BlockMap blocks;
	
	public Rigidbody rigidBody;
	public MeshRenderer renderer;
	
	private Mesh mesh;
	
	public bool hasCollision = true;
	public bool hasGravity = false;
	
	public Dictionary<IntVector2, GameObject> colliders = new Dictionary<IntVector2, GameObject>();
	public Shields shields = null;
	
	public Vector3 localCenter;
	
	// Use this for initialization
	void Awake () {
		blocks = new BlockMap(this);
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

	void OnDisable() {
		Ship.allActive.Remove(this);
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

		var newShipObj = Pool.Ship.TakeObject();
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
		
		avgPos.x /= blocks.Count;
		avgPos.y /= blocks.Count;
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
