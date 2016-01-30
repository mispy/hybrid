using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class RotatingTurret : BlockComponent {
	[HideInInspector]
	public new Collider collider { get; private set; }
	[HideInInspector]
	public LineRenderer dottedLine { get; private set; }
	[HideInInspector]
	public bool isBlocked { get; private set; }
    [HideInInspector]
    public bool showLine = false;
    CooldownCharger charger;
    ProjectileLauncher launcher;
    [ReadOnlyAttribute]
    public SpriteRenderer stickyOutBit = null;

	Vector2 origTextureScale;	
	Vector2 centerPoint;
    Vector2 targetPos;

	public Vector2 TipPosition {
		get {
            var point = (Vector2)stickyOutBit.transform.TransformPoint(Vector2.up*stickyOutBit.sprite.bounds.max.y);
            return point;
		}
	}

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        writer.Write(targetPos);
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        targetPos = reader.ReadVector2();
        UpdateTarget();
    }
	
	void Awake() {       
        charger = GetComponent<CooldownCharger>();
        launcher = GetComponent<ProjectileLauncher>();
		foreach (Transform child in transform) {
			stickyOutBit = child.GetComponent<SpriteRenderer>();
			break;
		}

        dottedLine = Pool.For("AimingLine").Attach<LineRenderer>(stickyOutBit.transform);
        dottedLine.transform.position = stickyOutBit.transform.position;
        dottedLine.gameObject.SetActive(true);
		dottedLine.SetWidth(0.5f, 0.5f);
        dottedLine.sortingLayerName = "UI";
        dottedLine.enabled = true;                
		
		RecalcCenterPoint();
		form.blocks.OnBlockAdded += OnBlockAdded;
		form.blocks.OnBlockRemoved += OnBlockRemoved;
		
		origTextureScale = dottedLine.material.mainTextureScale;

        syncRate = 0.2f;
        channel = Channel.Unreliable;
	}
	
	void OnBlockAdded(Block newBlock) {
		if (newBlock.type == block.type) RecalcCenterPoint();
	}
	
	void OnBlockRemoved(Block oldBlock) {
		if (oldBlock.type == block.type) RecalcCenterPoint();
	}
	
	void RecalcCenterPoint() {
		centerPoint = new Vector2(0.0f, 0.0f);
		var turrets = form.blocks.Find(block.type).ToList();
		foreach (var otherBlock in turrets) {
			centerPoint += form.BlockToLocalPos(otherBlock.pos);
		}
		centerPoint /= turrets.Count;
	}

    void UpdateTarget() {
        var targetDir = (targetPos-(Vector2)stickyOutBit.transform.position).normalized;
        var currentDir = stickyOutBit.transform.TransformDirection(Vector2.up);

        var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);
        stickyOutBit.transform.rotation = targetRotation;
        
        isBlocked = Util.TurretBlocked(form, TipPosition, targetPos);
        
        if (!isBlocked && showLine && charger.isReady) {
            dottedLine.enabled = true;

            var p1 = stickyOutBit.transform.InverseTransformPoint(TipPosition);
            var p2 = stickyOutBit.transform.InverseTransformPoint(targetPos);

            var timeToHit = Vector2.Distance(p1, p2) / launcher.launchVelocity;
            var velocityOffset = timeToHit * stickyOutBit.transform.InverseTransformVector(form.rigidBody.velocity);
            p2 = p2 + velocityOffset;
                
            dottedLine.SetVertexCount(2);
            dottedLine.SetPosition(0, p1);
            dottedLine.SetPosition(1, p2);           
            //Debug.LogFormat("{0} {1}", origTextureScale.x, (p2-p1).magnitude);
            dottedLine.material.mainTextureScale = new Vector2(origTextureScale.x*(p2-p1).magnitude*0.1f, origTextureScale.y);
        } else {
            dottedLine.enabled = false;
        }
    }

	public void AimTowards(Vector3 pos) {
		targetPos = transform.position + (pos - form.transform.TransformPoint(centerPoint));
        UpdateTarget();        
        SpaceNetwork.Sync(this);
    }

    void Update() {
        DebugUtil.DrawPoint(transform, TipPosition);
    }
}
