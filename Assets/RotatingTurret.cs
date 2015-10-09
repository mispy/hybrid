using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RotatingTurret : BlockComponent {
	[HideInInspector]
	public Blockform form { get; private set; }
	[HideInInspector]
	public Collider collider { get; private set; }
	[HideInInspector]
	public LineRenderer dottedLine { get; private set; }
	[HideInInspector]
	public bool isBlocked { get; private set; }

	Vector2 origTextureScale;
	
	Vector2 centerPoint;
	
	public Vector2 TipPosition {
		get {
			return (Vector2)transform.TransformPoint(Vector2.up*Tile.worldSize);
		}
	}
	
	void Start() {
		form = GetComponentInParent<Blockform>();
		dottedLine = GetComponent<LineRenderer>();
		dottedLine.SetWidth(0.1f, 0.1f);
		dottedLine.SetVertexCount(2);
		
		RecalcCenterPoint();
		form.blocks.OnBlockAdded += OnBlockAdded;
		form.blocks.OnBlockRemoved += OnBlockRemoved;
		
		origTextureScale = dottedLine.material.mainTextureScale;
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

	public Vector3 AimTowards(Vector3 targetPos) {
		targetPos = transform.position + (targetPos - form.transform.TransformPoint(centerPoint));
		var targetDir = (targetPos-transform.position).normalized;
		var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);
		transform.rotation = targetRotation;

		isBlocked = Util.TurretBlocked(form, transform.position, targetPos, 0.2f);

		return targetPos;
	}
	
	public void Update() {
		if (form.ship != Game.playerShip || !(Game.main.weaponSelect.selectedType == block.type)) {
			dottedLine.enabled = false;
			return;
		}
		
		var targetPos = AimTowards(Game.mousePos);
		
		if (!isBlocked) {
			dottedLine.enabled = true;
			var p1 = transform.InverseTransformPoint(TipPosition);
			var p2 = transform.InverseTransformPoint(targetPos);
			dottedLine.SetPosition(0, p1);
			dottedLine.SetPosition(1, p2);
			//Debug.LogFormat("{0} {1}", origTextureScale.x, (p2-p1).magnitude);
			dottedLine.material.mainTextureScale = new Vector2(origTextureScale.x*(p2-p1).magnitude*0.1f, origTextureScale.y);
		} else {
			dottedLine.enabled = false;
		}
	}
}
