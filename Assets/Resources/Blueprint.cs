using UnityEngine;
using System.Collections;

public class Blueprint : MonoBehaviour {
	public static GameObject prefab;
	public BlockMap blocks;
	public Ship ship;

	public MeshRenderer renderer;	
	private Mesh mesh;

	void Clear() {
		blocks = new BlockMap();
		blocks.OnBlockChanged = OnBlockChanged;
	}

	void Awake() {
		renderer = GetComponent<MeshRenderer>();
		mesh = GetComponent<MeshFilter>().mesh;	
		Clear();
	}

	void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (!gameObject.activeInHierarchy) return;
		UpdateMesh();
	}

	// Use this for initialization
	void OnEnable() {
		UpdateMesh();
	}

	void OnDisable() {
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
