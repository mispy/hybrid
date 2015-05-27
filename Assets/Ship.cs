using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockMap {
	public int width;
	public int height;
	public int centerX;
	public int centerY;

	private GameObject[,] blockArray;

	public BlockMap() {
		width = 128;
		height = 128;
		centerX = (int)Math.Floor(width/2.0);
		centerY = (int)Math.Floor(height/2.0);
		blockArray = new GameObject[width, height];
	}

	public IEnumerable All() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] != null) {
					yield return blockArray[x, y];
				}
			}
		}
	}

	public void Remove(GameObject obj) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (blockArray[x, y] == obj) {
					blockArray[x, y] = null;
				}
			}
		}
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

	public GameObject this[int x, int y] {
		get { return blockArray[centerX+x, centerY+y]; }
		set { blockArray[centerX+x, centerY+y] = value; }
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
			mass += 0.1f;
		}

		var rigid = GetComponent<Rigidbody2D>();
		rigid.mass = mass;
	}

	public void RemoveBlock(GameObject blockObj) {
		blocks.Remove(blockObj);
		RecalculateMass();
	}

	public void AddBlock(GameObject blockObj, int tileX, int tileY) {
		blocks[0,0] = blockObj;
		blockObj.transform.parent = gameObject.transform;
	}

	public void ReceiveImpact(Rigidbody2D fromRigid, GameObject blockObj) {
		// no point breaking off a single block from itself
		if (blocks.Count == 1) return;
		var myRigid = GetComponent<Rigidbody2D>();

		var impactVelocity = myRigid.velocity - fromRigid.velocity;
		var impactForce = impactVelocity.magnitude * fromRigid.mass;
		Debug.Log (impactForce);
		if (impactForce < 5) return;

		// break it off into a separate fragment
		var newShip = Instantiate(Game.main.shipPrefab, blockObj.transform.position, blockObj.transform.rotation) as GameObject;
		var newRigid = newShip.GetComponent<Rigidbody2D>();
		newRigid.velocity = myRigid.velocity;
		newRigid.angularVelocity = myRigid.angularVelocity;
		
		var newShipScript = newShip.GetComponent<Ship>();
		RemoveBlock(blockObj);
		newShipScript.AddBlock(blockObj, 0, 0);
	}


	void OnTriggerEnter2D(Collider2D collider) {
		var otherShip = collider.gameObject.transform.parent.GetComponent<Ship>();
		otherShip.ReceiveImpact(GetComponent<Rigidbody2D>(), collider.gameObject);
	}

	void OnParticleCollision(GameObject other) {

	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log(collision);
	}
}
