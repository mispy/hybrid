using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// renders a BlockMap as a collection of tiles
// draws large static structures very efficiently
public class TileRenderer : PoolBehaviour {
    BlockMap blocks;

    [HideInInspector]
    public TileLayer baseTiles;
    [HideInInspector]    
    public TileLayer topTiles;
    
    public IEnumerable<MeshRenderer> MeshRenderers {
        get {
            foreach (var chunk in baseTiles.AllChunks)
                yield return chunk.renderer;
            
            foreach (var chunk in topTiles.AllChunks)
                yield return chunk.renderer;
        }
    }
    
    public Bounds Bounds {
        get {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var chunk in baseTiles.AllChunks) {
                bounds.Encapsulate(chunk.renderer.bounds);
            }
            return bounds;
        }
    }
    
    public override void OnCreate() {
        var obj = Pool.For("TileLayer").TakeObject();
        obj.name = "TileLayer (Base)";
        obj.transform.parent = transform;
        obj.transform.position = transform.position;
        obj.SetActive(true);
        baseTiles = obj.GetComponent<TileLayer>();
        
        obj = Pool.For("TileLayer").TakeObject();
        obj.name = "TileLayer (Top)";
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
        obj.SetActive(true);
        topTiles = obj.GetComponent<TileLayer>();
    } 

    public void Start() {
        var form = GetComponent<Blockform>();
        if (form != null) SetBlocks(form.blocks);
    }

    void OnBlockAdded(Block block) {
        if (block.type.isComplexBlock && !block.IsBlueprint)
            return;

        var tileLayer = baseTiles;
        if (block.layer == BlockLayer.Top)
            tileLayer = topTiles;

        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                tileLayer[block.pos.x + i, block.pos.y + j] = block.type.tileable.GetRotatedTile(i, j, block.orientation);
            }
        }
    }

    void OnBlockRemoved(Block block) {
        var tileLayer = baseTiles;
        if (block.layer == BlockLayer.Top)
            tileLayer = topTiles;
        
        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                tileLayer[block.pos.x + i, block.pos.y + j] = null;
            }
        }
    }

    public void SetBlocks(BlockMap blocks) {
        this.blocks = blocks;
        blocks.OnBlockAdded += OnBlockAdded;
        blocks.OnBlockRemoved += OnBlockRemoved;

        foreach (var block in blocks.allBlocks)
            OnBlockAdded(block);
    }

    public void EnableRendering() {
        baseTiles.EnableRendering();
        topTiles.EnableRendering();
    }
    
    public void DisableRendering() {
        baseTiles.DisableRendering();
        topTiles.DisableRendering();
    }

    // Force a mesh update
    public void UpdateMesh() {
        foreach (var chunk in baseTiles.AllChunks)
            chunk.UpdateMesh();

        foreach (var chunk in topTiles.AllChunks)
            chunk.UpdateMesh();        
    }
}
