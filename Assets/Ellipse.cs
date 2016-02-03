using System;
using UnityEngine;
using System.Collections.Generic;

public class Ellipse {	
	public float width = 5f;
	public float height = 3f;
	public float theta = 45f;
    public float arcLength = 1f;
	public int resolution = 1000;	
	public Vector2[] positions;
    public Quaternion angleaxis;

    public Ellipse(float width, float height, int resolution = 100) {
		this.width = width;
		this.height = height;
		this.theta = 0;
        this.arcLength = arcLength;
		this.resolution = resolution;
        this.positions = new Vector2[resolution+1];
        this.angleaxis = Quaternion.AngleAxis(theta, Vector3.forward);

        for (int i = 0; i <= resolution; i++) {
			float angle = (float)i / (float)resolution * 2.0f * Mathf.PI * arcLength;
			positions[i] = new Vector2(width * Mathf.Cos (angle), height * Mathf.Sin (angle));
			positions[i] = (Vector2)(angleaxis * positions[i]);
		}
	}

    public IEnumerable<Vector2> ArcSlice(float fracStart, float fracEnd) {
        var startIndex = Mathf.FloorToInt(fracStart*positions.Length);
        var endIndex = Mathf.FloorToInt(fracEnd*positions.Length);
        for (var i = startIndex; i < endIndex; i++) {
            var index = Mathf.FloorToInt(Mathf.Repeat(i, positions.Length-1));
            yield return positions[index];
        }
    }

	public Ellipse Shrink(float amount) {
		return new Ellipse(width-amount, height-amount, resolution);
	}
}