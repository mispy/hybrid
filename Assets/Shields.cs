using UnityEngine;
using System.Collections;

public class Shields : MonoBehaviour {
	public LineRenderer lineRenderer;
	public Ellipse ellipse;
	public Blockform form;
	public Mesh mesh;
	public MeshCollider meshCollider;
	public MeshRenderer meshRenderer;

	void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
		ellipse = GetComponent<Ellipse>();
		form = GetComponentInParent<Blockform>();
		mesh = GetComponent<MeshFilter>().mesh;
		meshCollider = GetComponent<MeshCollider>();
		meshRenderer = GetComponent<MeshRenderer>();

		ellipse.centerX = 0;
		ellipse.centerY = 0;
		ellipse.theta = 0;
	}

	void UpdateMesh() {		
		var triangles = new int[ellipse.positions.Length*3*2];
		var vertices = new Vector3[ellipse.positions.Length*2];
		//var uvs = new Vector2[ellipse.positions.Length*2];
		for (var i = 0; i < ellipse.positions.Length; i += 2) {
			vertices[i] = (Vector3)ellipse.positions[i] + Vector3.forward*2;
			vertices[i+1] = (Vector3)ellipse.positions[i] + Vector3.back*2;
			//uvs[i] = (Vector3)ellipse.positions[i] + Vector3.forward*2;
			//uvs[i+1] = (Vector3)ellipse.positions[i] + Vector3.back*2;
		}
		
		for (var i = 0; i < vertices.Length-2; i++) {
			triangles[i*3] = i;
			triangles[i*3+1] = i+1;
			triangles[i*3+2] = i+2;
		}
		
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		//mesh.uv = uvs;
		mesh.Optimize();
		mesh.RecalculateBounds();
		//mesh.RecalculateNormals();
		// this null set here seems to be necessary to force the mesh collider
		// to update. not sure why! - mispy
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}

	void Update() {
		transform.localPosition = form.bounds.center;

		var width = form.bounds.size.x-3;
		var height = form.bounds.size.y-3;

		if (width != ellipse.width || height != ellipse.height) {
			ellipse.width = form.bounds.size.x-3;
			ellipse.height = form.bounds.size.y-3;
			ellipse.Redraw();
			UpdateMesh();
		}
	}
}
