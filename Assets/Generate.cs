using UnityEngine;
using System.Collections;
public class Generate : MonoBehaviour {
	public static Ship Asteroid(Vector2 pos, int radius) {
		var shipObj = Instantiate(Game.main.shipPrefab, pos, Quaternion.identity) as GameObject;
		var ship = shipObj.GetComponent<Ship>();
		for (var x = -radius; x < radius; x++) {
			for (var y = -radius; y < radius; y++) {
				if (Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) <= radius) {

					var ori = new Vector2[] { Vector2.up, Vector2.right, -Vector2.up, -Vector2.right };
					ship.SetBlock(x, y, Block.types["wall"], ori[Random.Range(0, 3)]);
				}
			}
		}
		ship.UpdateBlocks();
		return ship;
	}

	public static Ship TestShip(Vector2 pos) {
		var shipObj = Instantiate(Game.main.shipPrefab, pos, Quaternion.identity) as GameObject;
		var ship = shipObj.GetComponent<Ship>();

		ship.SetBlock(0, 0, Block.types["console"]);
		ship.SetBlock(0, 1, Block.types["wall"]);
		ship.SetBlock(-1, 1, Block.types["thruster"], -Vector2.right);
		ship.SetBlock(1, 1, Block.types["thruster"], Vector2.right);
		ship.SetBlock(0, -1, Block.types["thruster"], Vector2.up);
		ship.SetBlock(0, 2,  Block.types["thruster"], -Vector2.up);

		ship.UpdateBlocks();
		return ship;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
