﻿using UnityEngine;
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
		blocks.OnBlockAdded += OnBlockAdded;
	}

	public void OnEnable() {
		foreach (var chunk in blocks.baseTiles.AllChunks) {
			OnChunkCreated(chunk);
		}

		foreach (var chunk in blocks.topTiles.AllChunks) {
			OnChunkCreated(chunk);
		}

		blocks.baseTiles.OnChunkCreated += OnChunkCreated;
		blocks.topTiles.OnChunkCreated += OnChunkCreated;
	}

	public void OnBlockAdded(Block newBlock) {
		newBlock.ship = ship;
	}

	public void OnChunkCreated(TileChunk chunk) {
		chunk.renderer.material.color = Color.cyan;
	}

	public Block BlockAtWorldPos(Vector2 worldPos) {
		var block = blocks.Topmost(ship.WorldToBlockPos(worldPos));
		return block;
	}

	public override void OnRecycle() {
	}
}
