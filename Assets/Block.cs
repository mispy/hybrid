using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block : MonoBehaviour {
	public string orientation;
	public Block type;
	public static BlockPrefabs types = null;
	
	public static IEnumerable<Block> FindInRadius(Vector2 center, float radius) {
		var hits = Physics2D.OverlapCircleAll(center, radius);
		
		List<Block> nearbyBlocks = new List<Block>();
		
		foreach (var hit in hits) {
			if (hit.gameObject.transform.parent != null) {
				nearbyBlocks.Add(hit.gameObject.GetComponent<Block>());
				//Camera.main.transform.parent = activeShip.gameObject.transform;
			}
		}
		
		return nearbyBlocks.OrderBy(block => Vector2.Distance(center, block.transform.position));
	}

	public static Block Create(Block prefab) {
		var blockObj = Instantiate(prefab.gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		var block = blockObj.GetComponent<Block>();
		block.type = prefab;
		return block;
	}

	public Ship GetShip() {
		return transform.parent.GetComponent<Ship>();
	}

	void OnDestroy() {
		if (gameObject.transform.parent != null) {
			var ship = gameObject.transform.parent.GetComponent<Ship>();
			ship.RemoveBlock(this);
		}
	}
}
