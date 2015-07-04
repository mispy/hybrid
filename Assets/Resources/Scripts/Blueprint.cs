using UnityEngine;
using System.Collections;

public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public Material blueprintMaterial;

	[HideInInspector]
	public BlockMap blocks;
	[HideInInspector]
	public Ship ship;

	public override void OnCreate() {
		blocks = GetComponent<BlockMap>();
		blocks.OnBlockChanged = OnBlockChanged;
		blocks.OnChunkCreated = OnChunkCreated;
	}

	// Use this for initialization
	public void OnEnable() {
	}

	public void SetBlock(IntVector2 blockPos, BlueprintBlock block) {
		blocks[blockPos] = block;
	}

	public void OnBlockChanged(Block newBlock, Block oldBlock) {
		if (newBlock == null) return;

		newBlock.ship = ship;
	}

	public void OnChunkCreated(BlockChunk chunk) {
		//var texture = chunk.renderer.material.mainTexture;
		//chunk.renderer.material = blueprintMaterial;
		//chunk.renderer.material.mainTexture = texture;
		chunk.renderer.material.color = Color.cyan;
		blocks.DisableRendering();
	}

	public Block BlockAtWorldPos(Vector2 worldPos) {
		var block = blocks[ship.WorldToBlockPos(worldPos)];
		return block;
	}

	public override void OnRecycle() {
	}
}
