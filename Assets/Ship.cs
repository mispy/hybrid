﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockMap {
	public int width;
	public int height;
	public int centerX;
	public int centerY;

	private Block[,] blockArray;

	public BlockMap() {
		width = 128;
		height = 128;
		centerX = (int)Math.Floor(width/2.0);
		centerY = (int)Math.Floor(height/2.0);
		blockArray = new Block[width, height];
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
	
	public IEnumerable<Block> All() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] != null) {
					yield return blockArray[x, y];
				}
			}
		}
	}

	public IEnumerable<IntVector2> AllPositions() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] != null) {
					yield return new IntVector2(x-centerX, y-centerY);
				}
			}
		}
	}

	public int Count {
		get {
			var i = 0;
			foreach (var pos in AllPositions()) {
				i += 1;
			}
			return i;
		}
	}

	public Block this[int x, int y] {
		get { return blockArray[centerX+x, centerY+y]; }
		set {
			if (value != null) {
				value.pos.x = x;
				value.pos.y = y;
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

	public Rigidbody2D rigidBody;

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
		rigidBody = GetComponent<Rigidbody2D>();
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

	public void ReceiveImpact(Rigidbody2D fromRigid, Block block) {
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
		var newRigid = newShipObj.GetComponent<Rigidbody2D>();
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

	public void FireLasers() {
		foreach (var block in weaponBlocks.ToArray()) {
			Vector2 worldOrient;
			if (block.orientation == Vector2.up || block.orientation == -Vector2.up) {
				worldOrient = transform.TransformVector(block.orientation);
			} else {
				worldOrient = transform.TransformVector(-block.orientation);
			}

			ParticleSystem beam;
			if (particleCache.ContainsKey(block.pos)) {
				beam = particleCache[block.pos];
			} else {
				beam = Pool.ParticleBeam.TakeObject().GetComponent<ParticleSystem>();
				beam.gameObject.SetActive(true);
				beam.transform.parent = transform;
				beam.transform.localPosition = BlockToLocalPos(block.pos);
				beam.transform.up = worldOrient;
				particleCache[block.pos] = beam;
			}
					
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

	void OnParticleCollision(GameObject other) {

	}

	void OnCollisionEnter(Collision collision) {
	}

	// This first list contains every vertex of the mesh that we are going to render
	public List<Vector3> newVertices = new List<Vector3>();
	
	// The triangles tell Unity how to build each section of the mesh joining
	// the vertices
	public List<int> newTriangles = new List<int>();
	
	// The UV list is unimportant right now but it tells Unity how the texture is
	// aligned on each polygon
	public List<Vector2> newUV = new List<Vector2>();
	
	
	// A mesh is made up of the vertices, triangles and UVs we are going to define,
	// after we make them up we'll save them as this mesh
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
		foreach (var block in blocks.All()) {
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
		foreach (var block in blocks.All()) {
			mass += Block.mass;
		}
		
		rigidBody.mass = mass;
	}

	void UpdateColliders() {
		foreach (var colliderObj in colliders) {
			colliderObj.SetActive(false);
		}

		colliders.Clear();
		foreach (var block in blocks.All()) {
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
		foreach (var block in blocks.All()) {
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

		if (Block.tileWidth == 0) {
			Block.tileWidth = (float)Block.pixelSize / rend.material.mainTexture.width;
			Block.tileHeight = (float)Block.pixelSize / rend.material.mainTexture.height;
		}

		var tilesPerRow = Mathf.RoundToInt(rend.material.mainTexture.width / (float)Block.pixelSize);
		var tilesPerCol = Mathf.RoundToInt(rend.material.mainTexture.height / (float)Block.pixelSize);

		var index = new Vector2(Block.atlasBoxes[block.type].xMin, Block.atlasBoxes[block.type].yMin);

		mesh = GetComponent<MeshFilter>().mesh;

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
			newUV.Add(new Vector2 (index.x, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y));
			newUV.Add(new Vector2 (index.x, index.y));
		} else if (block.orientation == -Vector2.up) {			
			newUV.Add(new Vector2 (index.x, index.y));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x, index.y + Block.tileHeight));		
		} else if (block.orientation == Vector2.right) {
			newUV.Add(new Vector2 (index.x, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x, index.y));
			newUV.Add(new Vector2 ( index.x + Block.tileWidth, index.y));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y + Block.tileHeight));
		} else if (block.orientation == -Vector2.right) {
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y));
			newUV.Add(new Vector2 (index.x + Block.tileWidth, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x, index.y + Block.tileHeight));
			newUV.Add(new Vector2 (index.x, index.y));
		}
			
		squareCount++;
	}
}
