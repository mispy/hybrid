using UnityEngine;
using System.Collections;

public class Generate : MonoBehaviour {
	public static Ship Asteroid(Vector2 pos, int radius) {
		var shipObj = Instantiate(Game.main.shipPrefab, pos, Quaternion.identity) as GameObject;
		var ship = shipObj.GetComponent<Ship>();
		for (var x = -radius; x < radius; x++) {
			for (var y = -radius; y < radius; y++) {
				if (Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) <= radius) {
					ship.SetBlock(x, y, Block.types.wall);
				}
			}
		}
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
