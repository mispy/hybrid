using UnityEngine;
using System.Collections;

public class Crew : MonoBehaviour {
	public Block interactBlock = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		var rigid = GetComponent<Rigidbody2D>();

		if (Input.GetKeyDown(KeyCode.E) && Game.main.activeShip != null) {
			rigid.isKinematic = false;
			transform.parent = null;
			Game.main.activeShip = null;
			return;
		}

		if (Game.main.activeShip != null) return;

		/*if (Input.GetKeyDown(KeyCode.E) && interactBlock != null) {
			Game.main.activeShip = interactBlock.GetShip();
			rigid.isKinematic = true;
			transform.rotation = interactBlock.transform.rotation;
			transform.parent = Game.main.activeShip.gameObject.transform;
			return;
		}*/

		var speed = 3f;// * Time.deltaTime;
		Vector2 vel = rigid.velocity;

		if (Input.GetKey(KeyCode.W)) {
			vel.y = speed;
		}

		if (Input.GetKey(KeyCode.A)) {
			vel.x = -speed;
		}

		if (Input.GetKey(KeyCode.D)) {
			vel.x = speed;
		}

		if (Input.GetKey(KeyCode.S)) {
			vel.y = -speed;
		}

		rigid.velocity = vel;
		rigid.rotation = 0.0f;

		/*if (interactBlock != null) {
			interactBlock.GetComponent<SpriteRenderer>().color = Color.white;
		}
		interactBlock = null;

		var nearbyBlocks = Block.FindInRadius(transform.position, 0.2f);
		foreach (var block in nearbyBlocks) {
			if (block.type == Block.types.console) {
				interactBlock = block;
				interactBlock.GetComponent<SpriteRenderer>().color = Color.yellow;
			}
		}*/

	}
}
