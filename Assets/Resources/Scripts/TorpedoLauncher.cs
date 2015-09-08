using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TorpedoLauncher : BlockType {
	[HideInInspector]
	public Blockform form { get; private set; }
	[HideInInspector]
	public Collider collider { get; private set; }
	[HideInInspector]
	public LineRenderer dottedLine { get; private set; }

	public float timeBetweenShots = 0.1f;
	public float lastFireTime = 0f;

	Vector2 origTextureScale;

	Vector2 centerPoint;

	Vector3 TipPosition {
		get {
			return transform.TransformPoint(Vector3.up*Tile.worldSize);
		}
	}

	void Start() {
		form = GetComponentInParent<Blockform>();
		collider = form.GetComponent<ShipCollision>().colliders[block.pos].GetComponent<Collider>();
		dottedLine = GetComponent<LineRenderer>();
		dottedLine.SetWidth(0.3f, 0.3f);
		dottedLine.SetVertexCount(2);

		RecalcCenterPoint();
		form.blocks.OnBlockAdded += OnBlockAdded;
		form.blocks.OnBlockRemoved += OnBlockRemoved;

		origTextureScale = dottedLine.material.mainTextureScale;
	}

	void OnBlockAdded(Block newBlock) {
		if (newBlock.type is TorpedoLauncher) RecalcCenterPoint();
	}

	void OnBlockRemoved(Block oldBlock) {
		if (oldBlock.type is TorpedoLauncher) RecalcCenterPoint();
	}

	void RecalcCenterPoint() {
		centerPoint = new Vector2(0.0f, 0.0f);
		var launchers = form.blocks.Find<TorpedoLauncher>().ToList();
		foreach (var block in launchers) {
			centerPoint += form.BlockToLocalPos(block.pos);
		}
		centerPoint /= launchers.Count;
	}

	public Collider GetProbableHit(float maxDistance = 50f) {
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.up, out hit, maxDistance);	
		return hit.collider;
	}

	public void Fire() {	
		if (Time.time - lastFireTime < timeBetweenShots)
			return;


		lastFireTime = Time.time;


		var torpedo = Pool.For("Torpedo").TakeObject();
		torpedo.transform.position = TipPosition;
		torpedo.transform.rotation = transform.rotation;
		var mcol = torpedo.GetComponent<BoxCollider>();
		var rigid = torpedo.GetComponent<Rigidbody>();
		torpedo.SetActive(true);
		rigid.velocity = form.rigidBody.velocity;
		rigid.AddForce(transform.up*0.5f);
		Physics.IgnoreCollision(collider, mcol);
		if (form.shields) {
			Physics.IgnoreCollision(form.shields.GetComponent<Collider>(), mcol, true);
			Physics.IgnoreCollision(mcol, form.shields.GetComponent<Collider>(), true);
		}
	}

	public void Update() {
		if (form.ship != Game.playerShip || !(Game.main.weaponSelect.selectedType is TorpedoLauncher)) {
			dottedLine.enabled = false;
			return;
		}

		var targetPos = transform.position + (Game.mousePos - form.transform.TransformPoint(centerPoint));
		var targetDir = (targetPos-transform.position).normalized;
		var targetRotation = Quaternion.LookRotation(Vector3.forward, targetDir);
		transform.rotation = targetRotation;

		var isBlocked = Util.TurretBlocked(form, transform.position, targetPos, 0.3f);
		if (!isBlocked) {
			dottedLine.enabled = true;
			var p1 = transform.InverseTransformPoint(TipPosition);
			var p2 = transform.InverseTransformPoint(targetPos);
			dottedLine.SetPosition(0, p1);
			dottedLine.SetPosition(1, p2);
			//Debug.LogFormat("{0} {1}", origTextureScale.x, (p2-p1).magnitude);
			dottedLine.material.mainTextureScale = new Vector2(origTextureScale.x*(p2-p1).magnitude*0.1f, origTextureScale.y);

			if (Input.GetMouseButton(0)) {
				Fire();
			}
		} else {
			dottedLine.enabled = false;
		}
	}
}
