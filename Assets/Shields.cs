using UnityEngine;
using System.Collections;

public class Shields : PoolBehaviour {
	public Blockform form;
	public Ellipse ellipse;
	public ShieldCollider shieldCollider;

	public float maxHealth = 10000f;
	public float health = 10000f;
	public float regenRate = 10f;
	public bool isActive = false;
	
	public void TakeDamage(float amount) {
		health = Mathf.Max(0, health - amount);
		UpdateStatus();
	}

	void Awake() {
		form = GetComponentInParent<Blockform>();
		form.shields = this;
		shieldCollider = GetComponent<ShieldCollider>();
		form.blocks.OnBlockAdded += OnBlockUpdate;
		form.blocks.OnBlockRemoved += OnBlockUpdate;
	}

	void Start() {
		isActive = false;
		UpdateEllipse();
		UpdateStatus();
	}

	public void OnBlockUpdate(Block block) {		
		UpdateEllipse();
	}

	void UpdateEllipse() {
		transform.localPosition = form.bounds.center;
		
		var lineWidth = 1;
		var width = form.bounds.size.x-3;
		var height = form.bounds.size.y-3;
		
		if (ellipse == null || width != ellipse.width || height != ellipse.height) {
			ellipse = new Ellipse(0, 0, width, height, 0);
			SendMessage("OnShieldsResize");
		}
	}
	
	void UpdateStatus() {
		if (health >= maxHealth/4 && !isActive) {
			isActive = true;
			SendMessage("OnShieldsEnable");
		}

		if (health < maxHealth/4 && isActive) {
			isActive = false;
			SendMessage("OnShieldsDisable");
		}

		SendMessage("OnShieldsChange");
	}

	void Update() {
		health = Mathf.Min(maxHealth, health + regenRate*Time.deltaTime);
		UpdateStatus();

		if (!isActive)
			return;
	}
}
