using UnityEngine;
using System.Collections;

public class BlockPrefabs : MonoBehaviour {
	public Block wall;
	public Block thruster;
	public Block floor;
	public Block console;

	// Use this for initialization
	void Awake () {
		if (Block.types == null) {
			Block.types = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
