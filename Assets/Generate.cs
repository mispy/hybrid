using UnityEngine;
using System.Collections;
public class Generate : MonoBehaviour {
	public static Ship Asteroid(Vector2 pos, int radius) {
		var shipObj = Pool.Ship.TakeObject();
		var ship = shipObj.GetComponent<Ship>();
		for (var x = -radius; x < radius; x++) {
			for (var y = -radius; y < radius; y++) {
				if (Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) <= radius) {

					var ori = new Vector2[] { Vector2.up, Vector2.right, -Vector2.up, -Vector2.right };
					ship.SetBlock(x, y, Block.types["wall"], ori[Random.Range(0, 3)]);
				}
			}
		}
		shipObj.transform.position = pos;
		shipObj.SetActive(true);
		return ship;
	}

	public static Ship TestShip(Vector2 pos) {
		var shipObj = Pool.Ship.TakeObject();
		var ship = shipObj.GetComponent<Ship>();

		ship.SetBlock(0, 0, Block.types["console"], Vector2.up);
		ship.SetBlock(0, 1, Block.types["wall"]);
		ship.SetBlock(-1, 1, Block.types["wall"], -Vector2.right);
		ship.SetBlock(1, 1, Block.types["wall"], Vector2.right);
		ship.SetBlock(0, -1, Block.types["thruster"], Vector2.up);
		ship.SetBlock(0, 2, Block.types["wall"]);
		ship.SetBlock(1, 2,  Block.types["thruster"], -Vector2.up);
		ship.SetBlock(-1, 2,  Block.types["thruster"], -Vector2.up);
		ship.SetBlock(2, 2,  Block.types["wall"], -Vector2.up);
		ship.SetBlock(-2, 2,  Block.types["wall"], -Vector2.up);
		ship.SetBlock(2, 1,  Block.types["thruster"], Vector2.right);
		ship.SetBlock(-2, 1,  Block.types["thruster"], -Vector2.right);
		ship.SetBlock(0, 3, Block.types["laser"], -Vector2.up);
		shipObj.transform.Rotate(new Vector3(0, 0, 90));
		shipObj.transform.position = pos;
		shipObj.SetActive(true);

		return ship;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
