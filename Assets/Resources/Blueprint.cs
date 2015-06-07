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

	public void OnBeforeSerialize() {
	}

	public void OnAfterDeserialize() {
		blocks.OnBlockChanged = OnBlockChanged;
	}

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

	public override void OnFirstEnable() {
		UpdateMesh();
	}

	public override void OnRestoreEnable() {
		foreach (var data in blocks.saveData) {
			var block = Block.Deserialize(data);
			block.ship = ship;
			blocks[data.x, data.y] = block;
		}


		blocks.OnBlockChanged = OnBlockChanged;		
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
