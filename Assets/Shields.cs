using UnityEngine;
using System.Collections;

public class Shields : MonoBehaviour {
	public LineRenderer lineRenderer;
	public Ellipse ellipse;
	public Blockform form;

	void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
		ellipse = GetComponent<Ellipse>();
		form = GetComponentInParent<Blockform>();
	}

	void Update() {
		transform.localPosition = form.bounds.center;

		ellipse.centerX = 0;
		ellipse.centerY = 0;
		ellipse.width = form.bounds.size.x-3;
		ellipse.height = form.bounds.size.y-3;
		ellipse.theta = 0;
	}
}
