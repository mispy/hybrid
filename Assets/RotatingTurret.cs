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

	Vector2 origTextureScale;	
	Vector2 centerPoint;
    Vector2 targetPos;

	public Vector2 TipPosition {
		get {
			return (Vector2)transform.TransformPoint(Vector2.up*Tile.worldSize);
		}
	}

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        writer.Write(targetPos);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        targetPos = reader.ReadVector2();
        UpdateTarget();
    }
	
	void Awake() {       
        dottedLine = Pool.For("AimingLine").Attach<LineRenderer>(transform);
        dottedLine.transform.position = TipPosition;
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
        var targetDir = (targetPos-(Vector2)transform.position).normalized;
        var currentDir = transform.TransformDirection(Vector2.up);
        
        var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);
        transform.rotation = targetRotation;
        
        isBlocked = Util.TurretBlocked(form, transform.position, targetPos, 0.2f);
        
        if (!isBlocked && showLine) {
            dottedLine.enabled = true;
            var p1 = transform.InverseTransformPoint(TipPosition);
            var p2 = transform.InverseTransformPoint(targetPos);
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
}
