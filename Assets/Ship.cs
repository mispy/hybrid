using UnityEngine;
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
		width = 2056;
		height = 2056;
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
			if (this[neighbor] == null) {
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
			value.pos.x = x;
			value.pos.y = y;
			blockArray[centerX+x, centerY+y] = value; 
		}
	}

	public Block this[IntVector2 pos] {
		get { return blockArray[centerX+pos.x, centerY+pos.y]; }
		set { 
			value.pos = pos;
			blockArray[centerX+pos.x, centerY+pos.y] = value; 
		}
	}
}

public class Ship : MonoBehaviour {
	public BlockMap blocks;

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
	}

	public Block SetBlock(int x, int y, int blockType) {
		var block = new Block(this, blockType);
		blocks[x, y] = block;
		return block;
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

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		var localPos = transform.InverseTransformPoint(worldPos);
		return new IntVector2((int)Math.Round(localPos.x / Block.worldSize),
		                   (int)Math.Round(localPos.y / Block.worldSize));
	}

	public Vector2 BlockToLocalPos(IntVector2 blockPos) {
		return new Vector2(blockPos.x*Block.worldSize, blockPos.y*Block.worldSize);
	}

	public Vector2 BlockToWorldPos(IntVector2 blockPos) {
		return transform.TransformPoint(BlockToLocalPos(blockPos));
	}

	public void FireThrusters(string orientation) {
		/*var rigid = GetComponent<Rigidbody2D>();
		foreach (var block in blocks.All()) {
			if (block.type == Block.types.thruster && block.orientation == orientation) {
				var ps = block.GetComponent<ParticleSystem>();
				ps.Emit(1);
				rigid.AddForceAtPosition(block.transform.TransformVector(new Vector2(0, 1)), block.transform.position);
			}
		}*/
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.transform.parent == null)
			return;

		var otherShip = collider.gameObject.transform.parent.GetComponent<Ship>();
		otherShip.ReceiveImpact(GetComponent<Rigidbody2D>(), collider.gameObject.GetComponent<Block>());
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

	public List<BoxCollider2D> colliders;

	public void UpdateBlocks() {
		UpdateMesh();

		UpdateColliders();
		
		var mass = 0.0f;
		foreach (var block in blocks.All()) {
			mass += 0.0001f;
		}
		
		var rigid = GetComponent<Rigidbody2D>();
		rigid.mass = mass;
	}

	BoxCollider2D TakeCollider() {
		if (colliders.Count > 0) {
			var collider = colliders[0];
			colliders.RemoveAt(0);
			return collider;
		} else {
			var obj = Instantiate(Game.main.boxColliderPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			obj.transform.parent = transform;
			return obj.GetComponent<BoxCollider2D>();
		}
	}

	void UpdateColliders() {
		var newColliders = new List<BoxCollider2D>();

		foreach (var block in blocks.All()) {
			if (blocks.IsEdge(block.pos)) {
				var collider = TakeCollider();
				collider.transform.localPosition = BlockToLocalPos(block.pos);
				newColliders.Add(collider);
			}
		}

		// destroy any unneeded colliders
		foreach (var leftover in colliders) {
			Destroy(leftover.gameObject);
		}
		colliders = newColliders;
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

		// we want block tilesheets ordered left-right, then top-down
		// note that y axis in UV coordinates goes up, need to compensate
		var index = new Vector2(block.type % tilesPerRow, tilesPerCol - 1 - block.type / tilesPerRow);

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

		newUV.Add(new Vector2 (Block.tileWidth * index.x, Block.tileHeight * index.y + Block.tileHeight));
		newUV.Add(new Vector2 (Block.tileWidth * index.x + Block.tileWidth, Block.tileHeight * index.y + Block.tileHeight));
		newUV.Add(new Vector2 (Block.tileWidth * index.x + Block.tileWidth, Block.tileHeight * index.y));
		newUV.Add(new Vector2 (Block.tileWidth * index.x, Block.tileHeight * index.y));

		squareCount++;
	}
}
