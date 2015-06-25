using UnityEngine;
using System.Collections;

public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public BlockMap blocks;


	public MeshRenderer renderer;	
	private Mesh mesh;
	public Ship ship;

	public override void OnCreate() {
		renderer = GetComponent<MeshRenderer>();
		mesh = GetComponent<MeshFilter>().mesh;
		blocks = GetComponent<BlockMap>();
		blocks.OnBlockChanged = OnBlockChanged;
	}

	// Use this for initialization
	public void OnEnable() {
	}

	public void SetBlock(IntVector2 blockPos, BlueprintBlock block) {
		blocks[blockPos] = block;
	}

	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (newBlock != null)
			newBlock.ship = ship;
	}

	public Block BlockAtWorldPos(Vector2 worldPos) {
		var block = blocks[ship.WorldToBlockPos(worldPos)];
		return block;
	}

	public override void OnRecycle() {
	}
}
