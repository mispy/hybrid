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

	public void Remove(Block block) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] == block) {
					blockArray[x, y] = null;
				}
			}
		}
	}

	public IntVector2 Find(Block block) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] == block) {
					return new IntVector2(x-centerX, y-centerY);
				}
			}
		}

		throw new KeyNotFoundException();
	}

	public int Count {
		get {
			var i = 0;
			foreach (var block in All()) {
				i += 1;
			}
			return i;
		}
	}

	public Block this[int x, int y] {
		get { return blockArray[centerX+x, centerY+y]; }
		set { blockArray[centerX+x, centerY+y] = value; }
	}

	public Block this[IntVector2 pos] {
		get { return blockArray[centerX+pos.x, centerY+pos.y]; }
		set { blockArray[centerX+pos.x, centerY+pos.y] = value; }
	}
}

public class Ship : MonoBehaviour {
	public BlockMap blocks;

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
	}

	public void RecalculateMass() {
		var mass = 0.0f;
		foreach (var bp in blocks.AllPositions()) {
			mass += 1f;

			var col = blocks[bp].GetComponents<BoxCollider2D>()[0];
			var col2 = blocks[bp].GetComponents<BoxCollider2D>()[1];
			if (!blocks.IsEdge(bp)) {
				col.enabled = false;
				col2.enabled = false;
				Destroy(col);
				Destroy(col2);
			}
//			col.enabled = blocks.IsEdge(bp);

		}

		var rigid = GetComponent<Rigidbody2D>();
		rigid.mass = mass;
	}

	public void RemoveBlock(Block block) {
		blocks.Remove(block);
		RecalculateMass();
	}

	public void AddBlock(Block block, int tileX, int tileY) {
		blocks[tileX,tileY] = block;
		block.gameObject.transform.parent = gameObject.transform;
		block.transform.localPosition = BlockToLocalPos(new IntVector2(tileX, tileY));
	}

	public void ReceiveImpact(Rigidbody2D fromRigid, Block block) {
		// no point breaking off a single block from itself
		if (blocks.Count == 1) return;
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
		newShipScript.AddBlock(block, 0, 0);
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		var localPos = transform.InverseTransformPoint(worldPos);
		return new IntVector2((int)Math.Round(localPos.x / Game.main.tileWidth),
		                   (int)Math.Round(localPos.y / Game.main.tileHeight));
	}

	public Vector2 BlockToLocalPos(IntVector2 blockPos) {
		return new Vector2(blockPos.x*Game.main.tileWidth, blockPos.y*Game.main.tileHeight);
	}

	public Vector2 BlockToWorldPos(IntVector2 blockPos) {
		return transform.TransformPoint(BlockToLocalPos(blockPos));
	}

	public void FireThrusters(string orientation) {
		var rigid = GetComponent<Rigidbody2D>();
		foreach (var block in blocks.All()) {
			if (block.type == Block.types.thruster && block.orientation == orientation) {
				var ps = block.GetComponent<ParticleSystem>();
				ps.Emit(1);
				rigid.AddForceAtPosition(block.transform.TransformVector(new Vector2(0, 1)), block.transform.position);
			}
		}
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

	void Start() {
		// UV coordinates are specified in fractions of the total dimensions
		// since the dimensions of the tilesheet are much more likely to change than
		// the dimensions of an individual block, we recalculate the fractions we need
		mesh = GetComponent<MeshFilter>().mesh;
		var rend = GetComponent<MeshRenderer>();

		if (Block.tileWidth == 0) {
			Block.tileWidth = (float)Block.pixelSize / rend.material.mainTexture.width;
			Block.tileHeight = (float)Block.pixelSize / rend.material.mainTexture.height;
		}
		
		var i = 0;

		var tilesPerRow = Mathf.RoundToInt(rend.material.mainTexture.width / (float)Block.pixelSize);
		var tilesPerCol = Mathf.RoundToInt(rend.material.mainTexture.height / (float)Block.pixelSize);

		// we want block tilesheets ordered left-right, then top-down
		// note that y axis in UV coordinates goes up, need to compensate
		var index = new Vector2(i % tilesPerRow, tilesPerCol - 1 - i / tilesPerRow);

		Debug.Log(index);

		mesh = GetComponent<MeshFilter>().mesh;
		
		float x = transform.position.x;
		float y = transform.position.y;
		float z = transform.position.z;
		
		newVertices.Add(new Vector3 (x, y, z));
		newVertices.Add(new Vector3 (x + Block.worldSize, y, z));
		newVertices.Add(new Vector3 (x + Block.worldSize, y - Block.worldSize, z));
		newVertices.Add(new Vector3 (x , y - Block.worldSize, z));
		
		newTriangles.Add(0);
		newTriangles.Add(1);
		newTriangles.Add(3);
		newTriangles.Add(1);
		newTriangles.Add(2);
		newTriangles.Add(3);
		
		newUV.Add(new Vector2 (Block.tileWidth * index.x, Block.tileHeight * index.y + Block.tileHeight));
		newUV.Add(new Vector2 (Block.tileWidth * index.x + Block.tileWidth, Block.tileHeight * index.y + Block.tileHeight));
		newUV.Add(new Vector2 (Block.tileWidth * index.x + Block.tileWidth, Block.tileHeight * index.y));
		newUV.Add(new Vector2 (Block.tileWidth * index.x, Block.tileHeight * index.y));
		
		mesh.Clear ();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray(); // add this line to the code here
		mesh.Optimize ();
		mesh.RecalculateNormals ();
	}
}
