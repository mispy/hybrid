using UnityEngine;
using System.Collections;

public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public BlockMap blocks;


	public MeshRenderer renderer;	
	private Mesh mesh;
	public Ship ship;

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
		UpdateMesh();
	}

	public void SetBlock(IntVector2 blockPos, BlueprintBlock block) {
		blocks[blockPos] = block;
	}

	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (newBlock != null)
			newBlock.ship = ship;
		if (gameObject.activeInHierarchy)
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
