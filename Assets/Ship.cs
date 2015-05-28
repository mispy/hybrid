using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockMap {
	public int width;
	public int height;
	public int centerX;
	public int centerY;

	private Block[,] blockArray;

	public BlockMap() {
		width = 128;
		height = 128;
		centerX = (int)Math.Floor(width/2.0);
		centerY = (int)Math.Floor(height/2.0);
		blockArray = new Block[width, height];
	}

	public IEnumerable<Block> All() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] != null) {
					yield return blockArray[x, y];
				}
			}
		}
	}

	public void Remove(Block block) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] == block) {
					blockArray[x, y] = null;
				}
			}
		}
	}

	public IntVector2 Find(Block block) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] == block) {
					return new IntVector2(x-centerX, y-centerY);
				}
			}
		}

		throw new KeyNotFoundException();
	}

	public int Count {
		get {
			var i = 0;
			foreach (var block in All()) {
				i += 1;
			}
			return i;
		}
	}

	public Block this[int x, int y] {
		get { return blockArray[centerX+x, centerY+y]; }
		set { blockArray[centerX+x, centerY+y] = value; }
	}

	public Block this[IntVector2 pos] {
		get { return blockArray[centerX+pos.x, centerY+pos.y]; }
		set { blockArray[centerX+pos.x, centerY+pos.y] = value; }
	}
}

public class Ship : MonoBehaviour {
	public BlockMap blocks;

	// Use this for initialization
	void Awake () {
		blocks = new BlockMap();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void RecalculateMass() {
		var mass = 0.0f;
		foreach (var block in blocks.All()) {
			mass += 1f;
		}

		var rigid = GetComponent<Rigidbody2D>();
		rigid.mass = mass;
	}

	public void RemoveBlock(Block block) {
		blocks.Remove(block);
		RecalculateMass();
	}

	public void AddBlock(Block block, int tileX, int tileY) {
		blocks[tileX,tileY] = block;
		block.gameObject.transform.parent = gameObject.transform;
	}

	public void ReceiveImpact(Rigidbody2D fromRigid, Block block) {
		// no point breaking off a single block from itself
		if (blocks.Count == 1) return;
		var myRigid = GetComponent<Rigidbody2D>();

		var impactVelocity = myRigid.velocity - fromRigid.velocity;
		var impactForce = impactVelocity.magnitude * fromRigid.mass;
		if (impactForce < 5) return;

		// break it off into a separate fragment
		var newShip = Instantiate(Game.main.shipPrefab, block.transform.position, block.transform.rotation) as GameObject;
		var newRigid = newShip.GetComponent<Rigidbody2D>();
		newRigid.velocity = myRigid.velocity;
		newRigid.angularVelocity = myRigid.angularVelocity;
		
		var newShipScript = newShip.GetComponent<Ship>();
		RemoveBlock(block);
		newShipScript.AddBlock(block, 0, 0);
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		var localPos = transform.InverseTransformPoint(worldPos);
		return new IntVector2((int)Math.Round(localPos.x / Game.main.tileWidth),
		                   (int)Math.Round(localPos.y / Game.main.tileHeight));
	}

	public Vector2 BlockToLocalPos(IntVector2 blockPos) {
		return new Vector2(blockPos.x*Game.main.tileWidth, blockPos.y*Game.main.tileHeight);
	}

	public Vector2 BlockToWorldPos(IntVector2 blockPos) {
		return transform.TransformPoint(BlockToLocalPos(blockPos));
	}

	public void FireThrusters(string orientation) {
		var rigid = GetComponent<Rigidbody2D>();
		foreach (var block in blocks.All()) {
			if (block.type == Block.types.thruster && block.orientation == orientation) {
				var ps = block.GetComponent<ParticleSystem>();
				ps.Emit(1);
				rigid.AddForceAtPosition(block.transform.TransformVector(new Vector2(0, 1)), block.transform.position);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.transform.parent == null)
			return;

		var otherShip = collider.gameObject.transform.parent.GetComponent<Ship>();
		otherShip.ReceiveImpact(GetComponent<Rigidbody2D>(), collider.gameObject.GetComponent<Block>());
	}

	void OnParticleCollision(GameObject other) {

	}

	void OnCollisionEnter(Collision collision) {
	}
}
