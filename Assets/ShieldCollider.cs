using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Shields))]
[RequireComponent(typeof(MeshFilter))]
public class ShieldCollider : PoolBehaviour {
	public Mesh mesh;
	public MeshCollider meshCollider;
	public Shields shields;
	
	void Awake() {
		shields = GetComponent<Shields>();
		mesh = GetComponent<MeshFilter>().mesh;
	}

	public void OnShieldsEnable() {
		meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.convex = true;
		UpdateMesh(shields.ellipse);
	}

	public void OnShieldsDisable() {
		Destroy(meshCollider);
	}
	
	public void OnShieldsResize() {
		UpdateMesh(shields.ellipse);
	}

	public void UpdateMesh(Ellipse ellipse) {		
		if (meshCollider == null)
			return;

		var triangles = new int[ellipse.positions.Length*3*2];
		var vertices = new Vector3[ellipse.positions.Length*2];
		for (var i = 0; i < ellipse.positions.Length; i += 2) {
			vertices[i] = (Vector3)ellipse.positions[i] + Vector3.forward*2;
			vertices[i+1] = (Vector3)ellipse.positions[i] + Vector3.back*2;
		}
		
		for (var i = 0; i < vertices.Length-2; i++) {
			triangles[i*3] = i;
			triangles[i*3+1] = i+1;
			triangles[i*3+2] = i+2;
		}
		
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.Optimize();
		mesh.RecalculateBounds();
		
		// this null set here seems to be necessary to force the mesh collider
		// to update. not sure why! - mispy
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}
}