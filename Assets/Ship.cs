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

	public List<Vector3> meshVertices;
	public List<int> meshTriangles;
	public List<Vector2> meshUV;

	public BlockMap() {
		width = 1;
		height = 1;
		centerX = (int)Math.Floor(width/2.0);
		centerY = (int)Math.Floor(height/2.0);
		blockArray = new Block[width, height];	
		allBlocks = new List<Block>();
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

		var widthOffset = Mathf.FloorToInt((newWidth-width)/2.0f);
		var heightOffset = Mathf.FloorToInt((newHeight-height)/2.0f);
		
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				newBlocks[widthOffset+i, heightOffset+j] = blockArray[i,j];
			}
		}

		width = newWidth;
		height = newHeight;
		centerX = Mathf.FloorToInt(newWidth/2.0f);
		centerY = Mathf.FloorToInt(newHeight/2.0f);
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
			Debug.LogFormat("Expanding ship from {0},{1} to {2},{3} to encompass point at {4},{5}",
			                width, height, newWidth, newHeight, x, y);
			
			SetDimensions(newWidth, newHeight);
		}
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

			if (value == null) {
				var currentBlock = blockArray[centerX+x, centerY+y];

				if (currentBlock != null) {
					allBlocks.RemoveAt(currentBlock.index);
				}
			} else {
				value.pos.x = x;
				value.pos.y = y;
				value.index = allBlocks.Count;

				allBlocks.Add(value);
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

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
		rigidBody = GetComponent<Rigidbody>();
	}

	void Start() {
	}

	public Block SetBlock(int x, int y, int blockType) {
		var block = new Block(this, blockType);
		blocks[x, y] = block;
		return block;
	}

	public Block SetBlock(int x, int y, int blockType, Vector2 orientation) {
		var block = new Block(this, blockType);
		block.orientation = orientation;
		blocks[x, y] = block;
		return block;
	}

	public void AttachBlock(int x, int y, Block block) {
		block.ship = this;
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
		newShip.AttachBlock(0, 0, block);
		newShip.UpdateBlocks();
		var newRigid = newShipObj.GetComponent<Rigidbody>();
		newRigid.velocity = rigidBody.velocity;
		newRigid.angularVelocity = rigidBody.angularVelocity;
		newShipObj.SetActive(true);

		return newShip;
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		var localPos = transform.InverseTransformPoint(worldPos);
		return new IntVector2(Mathf.FloorToInt(localPos.x / Block.worldSize),
		                   Mathf.FloorToInt(localPos.y / Block.worldSize));
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

	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();
	public List<Vector2> newUV = new List<Vector2>();
	private Mesh mesh;

	private int squareCount;

	public List<GameObject> colliders;

	public bool hasCollision = true;
	public bool hasGravity = false;

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
		foreach (var block in blocks.All) {
			AlignBlockToMesh(block);
		}

		mesh.Clear();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.Optimize();
		mesh.RecalculateNormals();
		
		squareCount=0;
		newVertices.Clear();
		newTriangles.Clear();
		newUV.Clear();
	}

	void AlignBlockToMesh(Block block) {
		// UV coordinates are specified in fractions of the total dimensions
		// since the dimensions of the tilesheet are much more likely to change than
		// the dimensions of an individual block, we recalculate the fractions we need
		mesh = GetComponent<MeshFilter>().mesh;
		var rend = GetComponent<MeshRenderer>();

		var localPos = BlockToLocalPos(block.pos);
		float x = localPos.x;
		float y = localPos.y;
		float z = transform.position.z;
		
		newVertices.Add(new Vector3 (x - Block.worldSize / 2, y + Block.worldSize / 2, z));
		newVertices.Add(new Vector3 (x + Block.worldSize / 2, y + Block.worldSize / 2, z));
		newVertices.Add(new Vector3 (x + Block.worldSize / 2, y - Block.worldSize / 2, z));
		newVertices.Add(new Vector3 (x - Block.worldSize / 2, y - Block.worldSize / 2, z));
		
		newTriangles.Add(squareCount*4);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+3);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+2);
		newTriangles.Add((squareCount*4)+3);
			
		if (block.orientation == Vector2.up) {
			newUV.AddRange(Block.upUVs[block.type]);
		} else if (block.orientation == -Vector2.up) {
			newUV.AddRange(Block.downUVs[block.type]);
		} else if (block.orientation == -Vector2.right) {
			newUV.AddRange(Block.leftUVs[block.type]);
		} else if (block.orientation == Vector2.right) {
			newUV.AddRange(Block.rightUVs[block.type]);
		}

		squareCount++;
	}
}
