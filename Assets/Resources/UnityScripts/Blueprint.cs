using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blueprint : PoolBehaviour {
	public static GameObject prefab;
	public Material blueprintMaterial;

	private BlockMap _blocks;
	[HideInInspector]
	public BlockMap blocks {
		get {
			return _blocks;
		}
		set {
			_blocks = value;
			_blocks.OnBlockAdded += OnBlockAdded;
			tiles.SetBlocks(_blocks);
		}
	}
	[HideInInspector]
	public TileRenderer tiles;
	[HideInInspector]
	public Ship ship;

	public void Initialize(Ship ship) {
		this.ship = ship;
		blocks = ship.blueprintBlocks;
	}

	public override void OnCreate() {
		tiles = GetComponent<TileRenderer>();
	}

	public void Start() {
		foreach (var chunk in tiles.baseTiles.AllChunks) {
			OnChunkCreated(chunk);
		}

		foreach (var chunk in tiles.topTiles.AllChunks) {
			OnChunkCreated(chunk);
		}

		tiles.baseTiles.OnChunkCreated += OnChunkCreated;
		tiles.topTiles.OnChunkCreated += OnChunkCreated;
	}

	public void OnBlockAdded(Block newBlock) {
		newBlock.ship = ship;
	}

	public void OnChunkCreated(TileChunk chunk) {
		chunk.renderer.material.color = Color.cyan;
	}

	public IEnumerable<Block> BlocksAtWorldPos(Vector2 worldPos) {
		return blocks[ship.form.WorldToBlockPos(worldPos)];
	}

	public override void OnRecycle() {
	}
}
