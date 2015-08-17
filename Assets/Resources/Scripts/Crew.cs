using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public static Crew player;
	public static List<Crew> all = new List<Crew>();

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Constructor constructor;

	public int maxHealth = 100;
	public int health = 100;


	// Crew can be maglocked to a ship even if they don't have a current block
	// bc they attach to the sides as well
	public Ship maglockShip = null;
	public IntVector3 currentBlockPos;
	public IntVector3 maglockMoveBlockPos;
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

		Crew.all.Add(this);
	}

	void OnRecycle() {
		Crew.all.Remove(this);
	}

	public void MaglockMove(IntVector3 bp) {
		maglockMoveBlockPos = bp;
	}

	public void UseBlock(Block block)  {
		controlConsole = block;
	}

	void SetMaglock(Ship ship) {
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
	
	bool CanMaglock(Ship ship) {
		var blockPos = ship.WorldToBlockPos(transform.position);

		if (ship.blocks[blockPos] != null)
			return true;

		foreach (var bp in ship.blocks.NeighborsWithDiagonal(blockPos)) {
			if (ship.blocks[bp] != null)
				return true;
		}

		return false;
	}

	Ship FindMaglockShip() {
		if (maglockShip != null && CanMaglock(maglockShip)) {
			return maglockShip;
		} else {
			foreach (var ship in Ship.allActive) {
				if (CanMaglock(ship)) return ship;
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
		var ship = FindMaglockShip();

		if (maglockShip != null && ship != maglockShip)
			StopMaglock();

		if (maglockShip == null && ship != maglockShip)
			SetMaglock(ship);

		if (maglockShip != null) {
			currentBlockPos = maglockShip.WorldToBlockPos(transform.position);
			currentBlock = maglockShip.blocks[currentBlockPos];
		}

		if (maglockShip != null)
			UpdateMaglockMove();
	}

	void Update() {
		UpdateMaglock();
	}
}
