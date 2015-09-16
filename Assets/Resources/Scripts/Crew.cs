using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew {
	public Ship ship;
	public int maxHealth = 100;
	public int health = 100;
}

public class CrewBody : MonoBehaviour {
	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Constructor constructor;


	// Crew can be maglocked to a ship even if they don't have a current block
	// bc they attach to the sides as well
	public Blockform maglockForm = null;
	public IntVector2 currentBlockPos;
	public IntVector2 maglockMoveBlockPos;
	public Block currentBlock = null;

	public Block controlConsole = null;

	public CrewWeapon weapon;

	public void TakeDamage(int amount) {
		health -= amount;

		if (health <= 0)
			Pool.Recycle(this.gameObject);
	}

	void Awake() {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();
		weapon = GetComponent<CrewWeapon>();

		var obj = Pool.For("Constructor").TakeObject();	
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.SetActive(true);
		constructor = GetComponentInChildren<Constructor>();
	}

	public void MaglockMove(IntVector2 bp) {
		maglockMoveBlockPos = bp;
	}

	public void UseBlock(Block block)  {
		controlConsole = block;
	}

	void SetMaglock(Blockform form) {
		maglockForm = form;
		maglockForm.maglockedCrew.Add(this);
		transform.rotation = maglockForm.transform.rotation;
		transform.parent = maglockForm.transform;
		rigidBody.isKinematic = true;
		MaglockMove(form.WorldToBlockPos(transform.position));

		//if (this == Crew.player)
		//	maglockForm.blueprint.blocks.EnableRendering();
	}

	void StopMaglock() {
		gameObject.transform.parent = Game.main.transform;
		rigidBody.isKinematic = false;
		maglockForm.maglockedCrew.Remove(this);

		//if (this == Crew.player)
		//	maglockForm.blueprint.blocks.DisableRendering();

		maglockForm = null;
		currentBlock = null;
	}
	
	bool CanMaglock(Blockform form) {
		var blockPos = form.WorldToBlockPos(transform.position);

		if (form.blocks[blockPos, BlockLayer.Base] != null)
			return true;

		foreach (var bp in IntVector2.NeighborsWithDiagonal(blockPos)) {
			if (form.blocks[bp, BlockLayer.Base] != null)
				return true;
		}

		return false;
	}

	Blockform FindMaglockForm() {
		if (maglockForm != null && CanMaglock(maglockForm)) {
			return maglockForm;
		} else {
			foreach (var form in Game.activeSector.blockforms) {
				if (CanMaglock(form)) return form;
			}

			return null;
		}
	}
	
	void UpdateMaglockMove() {
		var speed = 15f;
		var targetPos = (Vector3)maglockForm.BlockToLocalPos(maglockMoveBlockPos);
		
		var pos = transform.localPosition;
		var dist = targetPos - pos;
		
		if (dist.magnitude > speed*Time.deltaTime) {
			dist.Normalize();
			dist = dist*speed*Time.deltaTime;
		}
		
		transform.localPosition += dist;
	}

	void UpdateMaglock() {
		var form = FindMaglockForm();

		if (maglockForm != null && form != maglockForm)
			StopMaglock();

		if (maglockForm == null && form != maglockForm)
			SetMaglock(form);

		if (maglockForm != null) {
			currentBlockPos = maglockForm.WorldToBlockPos(transform.position);
			currentBlock = maglockForm.blocks[currentBlockPos, BlockLayer.Base];
		}

		if (maglockForm != null)
			UpdateMaglockMove();
	}

	void Update() {
		UpdateMaglock();
	}
}
