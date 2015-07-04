using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Crew : MonoBehaviour {
	public static Crew player;

	public Rigidbody rigidBody;
	public BoxCollider collider;

	public Constructor constructor;
	


	// Crew can be maglocked to a ship even if they don't have a current block
	// bc they attach to the sides as well
	public Ship maglockShip = null;
	public IntVector2 currentBlockPos;
	public IntVector2 maglockMoveBlockPos;
	public Block currentBlock = null;

	public Block controlConsole = null;


	void Awake() {
		collider = GetComponent<BoxCollider>();
		rigidBody = GetComponent<Rigidbody>();

		var obj = Pool.For("Constructor").TakeObject();	
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.SetActive(true);
		constructor = GetComponentInChildren<Constructor>();
	}

	IEnumerator MaglockMoveCoroutine() {
		var speed = 0.2f;
		var targetPos = (Vector3)maglockShip.BlockToLocalPos(maglockMoveBlockPos);

		while (true) {
			if (maglockShip == null) break;

			var pos = transform.localPosition;
			var dist = targetPos - pos;

			if (dist.magnitude > speed) {
				dist.Normalize();
				dist = dist*speed;
			}

			transform.localPosition += dist;

			if (Vector3.Distance(transform.localPosition, targetPos) < Vector3.kEpsilon) {
				break;
			}

			yield return null;
		}
	}

	public void MaglockMove(IntVector2 bp) {
		maglockMoveBlockPos = bp;
		StopCoroutine("MaglockMoveCoroutine");
		StartCoroutine("MaglockMoveCoroutine");
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

		if (this == Crew.player)
			maglockShip.blueprint.blocks.EnableRendering();
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
	}

	void Update() {
		UpdateMaglock();
	}
}
