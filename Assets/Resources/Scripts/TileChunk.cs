using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileChunk : PoolBehaviour {
	[Tooltip("Width in tiles.")]
	[HideInInspector]
	public int width = 32;
	[Tooltip("Height in tiles."]
	[HideInInspector]
	public int height = 32;
	
	[HideInInspector]
	public MeshRenderer renderer;
	[HideInInspector]
	public Mesh mesh;
	
	public Block[] blockArray;
	private Vector3[] meshVertices;
	private Vector2[] meshUV;
	private int[] meshTriangles;
	
	private bool needMeshUpdate = false;
	
	public override void OnCreate() {
		renderer = GetComponent<MeshRenderer>();
		mesh = GetComponent<MeshFilter>().mesh;
		blockArray = new Block[width*height];
		meshVertices = new Vector3[width*height*4];
		meshUV = new Vector2[width*height*4];
		meshTriangles = new int[width*height*6];
	}
	
	void Start() {
		InvokeRepeating("UpdateMesh", 0.0f, 0.05f);
	}
	
	public IEnumerable<Vector3> GetVertices(Block block) {
		var lx = block.localX * Block.worldSize;
		var ly = block.localY * Block.worldSize;
		
		var m = block.PercentFilled;
		
		yield return new Vector3(lx - Block.worldSize*m / 2, ly + Block.worldSize*m / 2, 0);
		yield return new Vector3(lx + Block.worldSize*m / 2, ly + Block.worldSize*m / 2, 0);
		yield return new Vector3(lx + Block.worldSize*m / 2, ly - Block.worldSize*m / 2, 0);
		yield return new Vector3(lx - Block.worldSize*m / 2, ly - Block.worldSize*m / 2, 0);
	}
	
	public void AttachToMesh(Block block, int i) {	
		meshTriangles[i*6] = i*4;
		meshTriangles[i*6+1] = (i*4)+1;
		meshTriangles[i*6+2] = (i*4)+3;
		meshTriangles[i*6+3] = (i*4)+1;
		meshTriangles[i*6+4] = (i*4)+2;
		meshTriangles[i*6+5] = (i*4)+3;
		
		var verts = GetVertices(block).ToList();
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
	
	public IEnumerable<Block> AllBlocks {
		get {
			for (var i = 0; i < width; i++) {
				for (var j = 0; j < height; j++) {
					var block = blockArray[width * j + i];
					if (block != null) yield return block;
				}
			}
		}
	}
	
	public Block this[int x, int y] {
		get {
			var i = width * y + x;
			return blockArray[i];
		}
		
		set {
			Profiler.BeginSample("BlockChunk[x,y]");
			
			var i = width * y + x;
			blockArray[i] = value;
			
			if (value == null) {
				ClearMeshPos(i);
			} else {
				value.localX = x;
				value.localY = y;
				AttachToMesh(value, i);
			}
			
			QueueMeshUpdate();
			Profiler.EndSample();
		}
	}
	
	
	public void QueueMeshUpdate() {
		needMeshUpdate = true;
	}
	
	public void UpdateMesh() {
		if (!needMeshUpdate) return;
		
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.triangles = meshTriangles;
		mesh.uv = meshUV;
		mesh.Optimize();
		mesh.RecalculateNormals();	
		
		needMeshUpdate = false;
	}
}
