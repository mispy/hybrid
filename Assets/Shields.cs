using UnityEngine;
using System.Collections;

public class Shields : PoolBehaviour {
	public Blockform form;
	public Ellipse ellipse;
	public ShieldCollider shieldCollider;

	public float maxHealth = 10f;
	public float health = 10f;
	public float regenRate = 1f;

	void Awake() {
		form = GetComponentInParent<Blockform>();
		form.shields = this;
		shieldCollider = GetComponent<ShieldCollider>();
	}

	void Update() {
		transform.localPosition = form.bounds.center;
		
		var lineWidth = 1;
		var width = form.bounds.size.x-3;
		var height = form.bounds.size.y-3;
		
		if (ellipse == null || width != ellipse.width || height != ellipse.height) {
			ellipse = new Ellipse(0, 0, width, height, 0);
			shieldCollider.UpdateMesh(ellipse);
		}
	}
}
