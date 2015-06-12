using UnityEngine;
using System.Collections;

public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public BlockMap blocks;


	public MeshRenderer renderer;	
	private Mesh mesh;
	private Ship ship;

	public void Clear() {
		blocks = new BlockMap();
		blocks.OnBlockChanged = OnBlockChanged;
	}

	public override void OnCreate() {
		renderer = GetComponent<MeshRenderer>();
		mesh = GetComponent<MeshFilter>().mesh;	
		Clear();
	}

	// Use this for initialization
	public void OnEnable() {
		ship = gameObject.GetComponentInParent<Ship>();
		UpdateMesh();
	}

	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (!gameObject.activeInHierarchy) return;
		newBlock.scrapContent = 0;
		UpdateMesh();
	}

	public Block BlockAtWorldPos(Vector2 worldPos) {
		var block = blocks[ship.WorldToBlockPos(worldPos)];
		return block;
	}

	public override void OnRecycle() {
		Clear();
	}
	
	void UpdateMesh() {
		Profiler.BeginSample("UpdateMesh");
		
		mesh.Clear();
		mesh.vertices = blocks.meshVertices;
		mesh.triangles = blocks.meshTriangles;
		mesh.uv = blocks.meshUV;
		mesh.Optimize();
		mesh.RecalculateNormals();	

		Profiler.EndSample();
	}
}
