using UnityEngine;
using System.Collections;

[RequireComponent (typeof(LineRenderer))]
public class Ellipse : MonoBehaviour {
	
	public float width = 5;
	public float height = 3;
	public float centerX = 0;
	public float centerY = 0;
	public float theta = 45;
	public int resolution = 1000;
	
	private Vector2[] positions;

	public LineRenderer lineRenderer;
	
	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
	}

	void Update() {
		positions = CreateEllipse(width,height,centerX,centerY,theta,resolution);
		lineRenderer.SetVertexCount (resolution+1);
		for (int i = 0; i <= resolution; i++) {
			lineRenderer.SetPosition(i, positions[i]);
		}
	}
	
	Vector2[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution) {		
		positions = new Vector2[resolution+1];
		Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
		Vector2 center = new Vector2(h,k);
		
		for (int i = 0; i <= resolution; i++) {
			float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
			positions[i] = new Vector2(a * Mathf.Cos (angle), b * Mathf.Sin (angle));
			positions[i] = (Vector2)(q * positions[i]) + center;
		}
		
		return positions;
	}
}