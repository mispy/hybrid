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

	private Block[,] blockArray;
	private List<Block> allBlocks;

	public List<Vector3> meshVertices = new List<Vector3>();
	public List<int> meshTriangles = new List<int>();
	public List<Vector2> meshUV = new List<Vector2>();

	public Ship ship;

	public BlockMap(Ship ship) {
		width = 1;
		height = 1;
		centerX = width/2;
		centerY = height/2;
		blockArray = new Block[width, height];	
		allBlocks = new List<Block>();
		this.ship = ship;
	} 

	public IEnumerable<IntVector2> Neighbors(IntVector2 bp) {
		yield return new IntVector2(bp.x-1, bp.y);
		yield return new IntVector2(bp.x+1, bp.y);
		yield return new IntVector2(bp.x, bp.y-1);
		yield return new IntVector2(bp.x, bp.y+1);
	}
	
	public bool IsEdge(IntVector2 bp) {
		if (this[bp] == null) {
			return false;
		}
		
		foreach (var neighbor in Neighbors(bp)) {
			if (this[neighbor] == null || this[neighbor].collisionLayer != this[bp].collisionLayer) {
				return true;
			}
		}
		
		return false;
	}

	public List<Block> All {
		get {
			return allBlocks;
		}
	}

	public int Count {
		get {
			return allBlocks.Count;
		}
	}

	public bool WithinBounds(int x, int y) {
		if (centerX+x >= width || centerX+x < 0 || centerY+y >= height || centerY+y < 0)
			return false;

		return true;
	}

	public void SetDimensions(int newWidth, int newHeight) {
		var newBlocks = new Block[newWidth, newHeight];

		var widthOffset = (newWidth-width)/2;
		var heightOffset = (newHeight-height)/2;
		
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				newBlocks[widthOffset+i, heightOffset+j] = blockArray[i,j];
			}
		}

		width = newWidth;
		height = newHeight;
		centerX = newWidth/2;
		centerY = newHeight/2;
		blockArray = newBlocks;
	}

	// dynamically reallocate array size to encompass the given point as needed
	// width/height are always a power of two
	public void ExpandToEncompass(int x, int y) {
		var newWidth = width;
		var newHeight = height;
		var newCenterX = centerX;
		var newCenterY = centerY;

		while (newCenterX+x >= newWidth || newCenterX+x < 0) {
			newWidth = newWidth << 1;
			newCenterX = Mathf.FloorToInt(newWidth/2.0f);
		}

		while (newCenterY+y >= newHeight || newCenterY+y < 0) {
			newHeight = newHeight << 1;
			newCenterY = Mathf.FloorToInt(newHeight/2.0f);
		}

		if (newWidth != width || newHeight != height) {
			//Debug.LogFormat("Expanding ship from {0},{1} to {2},{3} to encompass point at {4},{5}",
			//                width, height, newWidth, newHeight, x, y);
			
			SetDimensions(newWidth, newHeight);
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

	public Block this[int x, int y] {
		get { 
			if (WithinBounds(x, y)) {
				return blockArray[centerX+x, centerY+y]; 
			} else {
				return null;
			}
		}
		set {
			ExpandToEncompass(x, y);

			var currentBlock = blockArray[centerX+x, centerY+y];

			if (value == null && currentBlock == null) {
				return;
			} else if (value == null && currentBlock != null) {
				// removing an existing block
				meshTriangles.RemoveRange(meshTriangles.Count - 6, 6);
				meshVertices.RemoveRange(currentBlock.index, 4);
				meshUV.RemoveRange(currentBlock.index, 4);
				allBlocks.RemoveAt(currentBlock.index);
				for (var i = currentBlock.index; i < allBlocks.Count; i++) {
					allBlocks[i].index = i;
				}
			} else if (value != null && currentBlock == null) {
				// adding a new block
				value.index = allBlocks.Count;
				allBlocks.Add(value);

				meshTriangles.Add(value.index*4);
				meshTriangles.Add((value.index*4)+1);
				meshTriangles.Add((value.index*4)+3);
				meshTriangles.Add((value.index*4)+1);
				meshTriangles.Add((value.index*4)+2);
				meshTriangles.Add((value.index*4)+3);

				meshVertices.AddRange(GetVertices(x, y));
				meshUV.AddRange(Block.GetUVs(value));
			} else if (value != null && currentBlock != null) {
				// replacing an existing block
				value.index = currentBlock.index;
				allBlocks[value.index] = value;

				var uvs = Block.GetUVs(value);
				meshUV[value.index*4] = uvs[0];
				meshUV[value.index*4 + 1] = uvs[1];
				meshUV[value.index*4 + 2] = uvs[2];
				meshUV[value.index*4 + 3] = uvs[3];

				var vertices = GetVertices(x, y).ToArray();
				meshVertices[value.index*4] = vertices[0];
				meshVertices[value.index*4 + 1] = vertices[1];
				meshVertices[value.index*4 + 2] = vertices[2];
				meshVertices[value.index*4 + 3] = vertices[3];
			}

			if (value != null) {
				value.pos.x = x;
				value.pos.y = y;
				value.ship = ship;
			}

			blockArray[centerX+x, centerY+y] = value; 
		}
	}

	public Block this[IntVector2 pos] {
		get { return this[pos.x, pos.y]; }
		set { this[pos.x, pos.y] = value; }
	}
}

public class Ship : MonoBehaviour {
	public BlockMap blocks;
	public List<Block> thrusterBlocks = new List<Block>();
	public List<Block> weaponBlocks = new List<Block>();

	public Rigidbody rigidBody;

	
	private Mesh mesh;
	
	public List<GameObject> colliders;
	
	public bool hasCollision = true;
	public bool hasGravity = false;

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap(this);
		rigidBody = GetComponent<Rigidbody>();
		mesh = GetComponent<MeshFilter>().mesh;		
	}

	void Start() {
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
		foreach (var block in thrusterBlocks) {
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
				UpdateBlocks();
			}
		}
	}

	public void FireLasers() {
		foreach (var block in weaponBlocks.ToArray()) {
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

				ship.UpdateBlocks();
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

	public void UpdateBlocks() {
		UpdateMesh();

		if (hasCollision) {
			UpdateColliders();
		}

		thrusterBlocks.Clear();
		weaponBlocks.Clear();
		foreach (var block in blocks.All) {
			if (block.type == Block.types["thruster"]) {
				thrusterBlocks.Add(block);
			}

			if (block.type == Block.types["laser"]) {
				weaponBlocks.Add(block);
			}

			if (block.type == Block.types["console"]) {
				hasGravity = true;
			}
		}

		var mass = 0.0f;
		foreach (var block in blocks.All) {
			mass += Block.mass;
		}
		
		rigidBody.mass = mass;
	}

	void UpdateColliders() {
		foreach (var colliderObj in colliders) {
			colliderObj.SetActive(false);
		}

		colliders.Clear();
		foreach (var block in blocks.All) {
			if (blocks.IsEdge(block.pos)) {
				GameObject colliderObj;
				if (block.collisionLayer == Block.wallLayer)
					colliderObj = Pool.WallCollider.TakeObject();
				else
					colliderObj = Pool.FloorCollider.TakeObject();
				colliderObj.SetActive(true);
				colliderObj.transform.parent = transform;
				colliderObj.transform.localPosition = BlockToLocalPos(block.pos);
				colliderObj.layer = block.collisionLayer;
				colliders.Add(colliderObj);
			}
		}
	}

	void UpdateMesh() {
		//mesh.Clear();
		mesh.vertices = blocks.meshVertices.ToArray();
		mesh.triangles = blocks.meshTriangles.ToArray();
		mesh.uv = blocks.meshUV.ToArray();
		//mesh.Optimize();
		//mesh.RecalculateNormals();		
	}
}
