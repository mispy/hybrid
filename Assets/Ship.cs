using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ship : MonoBehaviour {
	public Dictionary<KeyValuePair<int,int>, GameObject> currentTiles = new Dictionary<KeyValuePair<int,int>, GameObject>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter2D(Collider2D collider) {
		// break it off into a separate fragment
		var obj = collider.gameObject;

		var newShip = Instantiate(Game.main.shipTemplate, obj.transform.position, obj.transform.rotation) as GameObject;
		var newRigid = newShip.GetComponent<Rigidbody2D>();

		newRigid.velocity = collider.attachedRigidbody.velocity;
		newRigid.angularVelocity = collider.attachedRigidbody.angularVelocity;

		obj.transform.parent = newShip.transform;


	}

	void OnParticleCollision(GameObject other) {

	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log(collision);
	}
}
