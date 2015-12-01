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

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        writer.Write(health);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        health = reader.ReadSingle();
        UpdateStatus();
        SendMessage("OnShieldsChange");
    }

	public void TakeDamage(float amount) {
		health = Mathf.Max(0, health - amount);
		UpdateStatus();
		SendMessage("OnShieldsChange");

        if (SpaceNetwork.isServer)
            SpaceNetwork.Sync(this);
	}

	void Awake() {
		form = GetComponentInParent<Blockform>();
		form.shields = this;
		shieldCollider = GetComponent<ShieldCollider>();
		form.blocks.OnBlockAdded += OnBlockUpdate;
		form.blocks.OnBlockRemoved += OnBlockUpdate;
        isActive = false;
	}

	void Start() {
		UpdateEllipse();
		UpdateStatus();
	}

	public void OnBlockUpdate(Block block) {		
		UpdateEllipse();
	}

	void UpdateEllipse() {
        transform.rotation = form.transform.rotation;
		transform.localPosition = form.localBounds.center;
		
		var width = form.localBounds.size.x-3;
		var height = form.localBounds.size.y-3;
		
		if (ellipse == null || width != ellipse.width || height != ellipse.height) {
			ellipse = new Ellipse(0, 0, width, height, 0);
			SendMessage("OnShieldsResize");
		}
	}
	
	public void UpdateStatus() {
        var powered = false;
        foreach (var generator in form.blocks.Find<ShieldGenerator>()) {
            if (generator.isPowered) powered = true;
        }

        if (!powered) {
            if (isActive) {
                isActive = false;
                SendMessage("OnShieldsDisable");
            }
            return;
        }

		if (health >= maxHealth/2 && !isActive) {
			isActive = true;
			SendMessage("OnShieldsEnable");
		}

		if (health <= 0 && isActive) {
			isActive = false;
			SendMessage("OnShieldsDisable");
		}
	}

	void Update() {
        foreach (var block in form.blocks.Find<ShieldGenerator>()) {
            if (!block.isPowered) continue;

            if (health < maxHealth) {
                health = Mathf.Min(maxHealth, health + regenRate*Time.deltaTime);       
                UpdateStatus();
                SendMessage("OnShieldsChange");
            }
        }

		if (!isActive)
			return;
	}
}
