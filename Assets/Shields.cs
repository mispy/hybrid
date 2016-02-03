using UnityEngine;
using System.Collections;
using System.Linq;

public class Shields : PoolBehaviour {
	public Blockform form;
	public Ellipse ellipse;
	public ShieldCollider shieldCollider;

	public float maxHealth = 100000f;
	public float health = 100000f;
	public float regenRate = 100f;
    float _angle = 0f;
    public float angle {
        get {
            return _angle;
        }

        set {
            _angle = Mathf.Repeat(value, 1f);
            UpdateArc();
        }
    }
    public float arcLength = 0.2f;
	public bool isActive = false;
    public Vector2[] arcPositions;

    public bool hasAIControl {
        get {
            foreach (var block in form.blocks.Find<ShieldGenerator>()) {
                if (block.crew != null && block.crew.mind != null)
                    return true;
            }

            return false;
        }
    }

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        writer.Write(health);
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        health = reader.ReadSingle();
        UpdateStatus();
        SendMessage("OnShieldsChange");
    }

	public void TakeDamage(float amount) {
        return;
		health = Mathf.Max(0, health - amount);
		UpdateStatus();
		SendMessage("OnShieldsChange");

        //if (SpaceNetwork.isServer)
        //    SpaceNetwork.Sync(this);
	}

	void Awake() {
		form = GetComponentInParent<Blockform>();
		form.shields = this;
		shieldCollider = GetComponent<ShieldCollider>();
        isActive = false;
	}
        
	void Start() {
        form.blocks.OnBlockAdded += OnBlockUpdate;
        form.blocks.OnBlockRemoved += OnBlockUpdate;
		UpdateEllipse();
		UpdateStatus();
	}

	public void OnBlockUpdate(Block block) {		
		UpdateEllipse();
	}

	void UpdateEllipse() {
        transform.rotation = form.transform.rotation;
		transform.localPosition = form.localBounds.center;
		
		var width = form.localBounds.size.x-2;
		var height = form.localBounds.size.y-2;
		
		if (ellipse == null || width != ellipse.width || height != ellipse.height) {
			ellipse = new Ellipse(width, height);
			SendMessage("OnShieldsResize");
            UpdateArc();
		}        
	}

    void UpdateArc() {
        arcPositions = ellipse.ArcSlice(angle, angle+arcLength).ToArray();
        SendMessage("OnShieldsMove");
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
