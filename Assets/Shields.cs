using UnityEngine;
using System.Collections;

public class Shields : PoolBehaviour {
	public Blockform form;
	public Ellipse ellipse;
	public ShieldCollider shieldCollider;

	public float maxHealth = 10000f;
	public float health = 10000f;
	public float regenRate = 10f;
	public bool isActive = true;
	
	public void TakeDamage(float amount) {
		health = Mathf.Max(0, health - amount);
		
		if (health < 5f) {
			isActive = false;
		}
	}

	void Awake() {
		form = GetComponentInParent<Blockform>();
		form.shields = this;
		shieldCollider = GetComponent<ShieldCollider>();
	}

	void Update() {
		health = Mathf.Min(maxHealth, health + regenRate*Time.deltaTime);

		if (health >= maxHealth/4) {
			isActive = true;
		}

		if (!isActive)
			return;

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
