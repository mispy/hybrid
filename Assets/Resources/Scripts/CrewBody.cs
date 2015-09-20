using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CrewBody : PoolBehaviour {
	public Crew crew;
	public Rigidbody rigidBody;
	public BoxCollider collider;
	public Constructor constructor;
	
	
	// Crew can be maglocked to a ship even if they don't have a current block
	// bc they attach to the sides as well
	public Blockform maglockShip = null;
	public IntVector2 currentBlockPos;
	public IntVector2 maglockMoveBlockPos;
	public Block currentBlock = null;
	
	public Block controlConsole = null;
	
	public CrewWeapon weapon;
	
	public void TakeDamage(int amount) {
		crew.health -= amount;
		
		if (crew.health <= 0)
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
	
	public void SetMaglock(Blockform ship) {
		maglockShip = ship;
		maglockShip.maglockedCrew.Add(this);
		transform.rotation = maglockShip.transform.rotation;
		transform.parent = maglockShip.transform;
		rigidBody.isKinematic = true;
		MaglockMove(ship.WorldToBlockPos(transform.position));
		
		//if (this == Crew.player)
		//	maglockShip.blueprint.blocks.EnableRendering();
	}
	
	void StopMaglock() {
		gameObject.transform.parent = Game.main.transform;
		rigidBody.isKinematic = false;
		maglockShip.maglockedCrew.Remove(this);
		
		//if (this == Crew.player)
		//	maglockShip.blueprint.blocks.DisableRendering();
		
		maglockShip = null;
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
	
	Blockform FindmaglockShip() {
		if (maglockShip != null && CanMaglock(maglockShip)) {
			return maglockShip;
		} else {
			foreach (var form in Game.activeSector.blockforms) {
				if (CanMaglock(form)) return form;
			}
			
			return null;
		}
	}
	
	void UpdateMaglockMove() {
		var speed = 15f;
		var targetPos = (Vector3)maglockShip.BlockToLocalPos(maglockMoveBlockPos);
		
		var pos = transform.localPosition;
		var dist = targetPos - pos;
		
		if (dist.magnitude > speed*Time.deltaTime) {
			dist.Normalize();
			dist = dist*speed*Time.deltaTime;
		}
		
		transform.localPosition += dist;
	}
	
	void UpdateMaglock() {
		var form = FindmaglockShip();
		
		if (maglockShip != null && form != maglockShip)
			StopMaglock();
		
		if (maglockShip == null && form != maglockShip)
			SetMaglock(form);
		
		if (maglockShip != null) {
			currentBlockPos = maglockShip.WorldToBlockPos(transform.position);
			currentBlock = maglockShip.blocks[currentBlockPos, BlockLayer.Base];
		}
		
		if (maglockShip != null)
			UpdateMaglockMove();
	}
	
	void Update() {
		UpdateMaglock();
	}
}
