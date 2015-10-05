using System;
using UnityEngine;

public class Ellipse {	
	public float width = 5;
	public float height = 3;
	public Vector2 center;
	public float theta = 45;
	public int resolution = 1000;	
	public Vector2[] positions;

	public Ellipse(float centerX, float centerY, float width, float height, float theta, int resolution = 1000) {
		this.center = new Vector2(centerX, centerY);
		this.width = width;
		this.height = height;
		this.theta = theta;
		this.resolution = resolution;
		this.positions = new Vector2[resolution+1];

		Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);

		for (int i = 0; i <= resolution; i++) {
			float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
			positions[i] = new Vector2(width * Mathf.Cos (angle), height * Mathf.Sin (angle));
			positions[i] = (Vector2)(q * positions[i]) + center;
		}
	}

	public Ellipse Shrink(float amount) {
		return new Ellipse(center.x, center.y, width-amount, height-amount, theta, resolution);
	}
}