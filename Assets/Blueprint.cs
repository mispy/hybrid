using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blueprint : PoolBehaviour {
    public static GameObject prefab;
    public Material blueprintMaterial;
    [HideInInspector]
    public Blockform ship;

    private BlockMap _blocks;
    [HideInInspector]
    public BlockMap blocks {
        get {
            return _blocks;
        }
        set {
            _blocks = value;
            tiles.SetBlocks(_blocks);
        }
    }
    [HideInInspector]
    public TileRenderer tiles;

    public override void OnCreate() {
        tiles = GetComponent<TileRenderer>();
    }

    public void Awake() {
        ship = GetComponentInParent<Blockform>();
    }

    public void Initialize() {
        blocks = Pool.For("BlockMap").Attach<BlockMap>(transform);
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

    public void OnChunkCreated(TileChunk chunk) {
        chunk.renderer.material.color = Color.cyan;
    }

    public IEnumerable<Block> BlocksAtWorldPos(Vector2 worldPos) {
        return blocks.BlocksAtPos(ship.WorldToBlockPos(worldPos));
    }

    public override void OnRecycle() {
    }

    public IntVector2 WorldToBlockPos(Vector2 worldPos) {
        return LocalToBlockPos(transform.InverseTransformPoint(worldPos));
    }
    
    public IntVector2 LocalToBlockPos(Vector3 localPos) {
        // remember that blocks go around the center point of the center block at [0,0]        
        return new IntVector2(Mathf.FloorToInt((localPos.x + Tile.worldSize/2.0f) / Tile.worldSize),
                              Mathf.FloorToInt((localPos.y + Tile.worldSize/2.0f) / Tile.worldSize));
    }
    
    
    public Vector2 BlockToLocalPos(IntVector2 blockPos) {
        return new Vector2(blockPos.x*Tile.worldSize, blockPos.y*Tile.worldSize);
    }
    
    public Vector2 BlockToLocalPos(Block block) {
        var centerX = block.pos.x + block.Width/2.0f;
        var centerY = block.pos.y + block.Height/2.0f;
        return new Vector2(centerX * Tile.worldSize - Tile.worldSize/2.0f, centerY * Tile.worldSize - Tile.worldSize/2.0f);
    }
    
    public Vector2 BlockToWorldPos(IntVector2 blockPos) {
        return transform.TransformPoint(BlockToLocalPos(blockPos));
    }
    
    public Vector2 BlockToWorldPos(Block block) {
        return transform.TransformPoint(BlockToLocalPos(block));
    }
    
    public IEnumerable<Block> BlocksInLocalRadius(Vector2 localPos, float radius) {
        IntVector2 blockPos = LocalToBlockPos(localPos);
        int blockRadius = Mathf.RoundToInt(radius/Tile.worldSize);
        
        for (var i = -blockRadius; i <= blockRadius; i++) {
            for (var j = -blockRadius; j <= blockRadius; j++) {
                var pos = new IntVector2(blockPos.x + i, blockPos.y + j);
                if (IntVector2.Distance(pos, blockPos) <= blockRadius) {
                    foreach (var block in blocks.BlocksAtPos(pos))
                        yield return block;
                }
            }
        }
    }
    

}
