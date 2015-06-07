using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public BlockMap blocks;
	private Ship ship;

	public MeshRenderer renderer;	
	private Mesh mesh;

	public void Clear() {
		blocks = new BlockMap();
		blocks.OnBlockChanged = OnBlockChanged;
	}

	public override void OnCreate() {
		renderer = GetComponent<MeshRenderer>();
		mesh = GetComponent<MeshFilter>().mesh;	
		Clear();
	}

	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (!gameObject.activeInHierarchy) return;
		UpdateMesh();
	}

	// Use this for initialization
	public void OnEnable() {
		UpdateMesh();
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
