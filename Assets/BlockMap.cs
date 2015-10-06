using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BlockMap {
	public Ship ship;

	// Calculated dimensions
    public int maxX;
    public int minX;
    public int maxY;
    public int minY;
	public int width;
	public int height;
	public Rect boundingRect;
	// Total number of filled base blocks
	public int size;

	// Values used for translating between 0,0 center to traditional
	// array coordinates
	int centerBlockX;
    int centerBlockY;
    int centerChunkX;
    int centerChunkY;
    int chunkWidth;
    int chunkHeight;
    int widthInChunks;
    int heightInChunks;
    BlockChunk[,] baseChunks;
    BlockChunk[,] topChunks;


    public Dictionary<Type, List<Block>> blockTypeCache = new Dictionary<Type, List<Block>>();

    public delegate void BlockAddedHandler(Block newBlock);
    public delegate void BlockRemovedHandler(Block oldBlock);
    public event BlockAddedHandler OnBlockAdded;
    public event BlockRemovedHandler OnBlockRemoved;

    public delegate void ChunkCreatedHandler(BlockChunk newChunk);
    public event ChunkCreatedHandler OnChunkCreated;

    public BlockMap(Ship ship) {
		this.ship = ship;

        minX = 0;
        minY = 0;
        maxX = 0;
        maxY = 0;
		width = 0;
		height = 0;
		size = 0;

        chunkWidth = 32;
        chunkHeight = 32;
        widthInChunks = 16;
        heightInChunks = 16;
        baseChunks = new BlockChunk[chunkWidth, chunkHeight];
        topChunks = new BlockChunk[chunkWidth, chunkHeight];


        centerChunkX = widthInChunks/2;
        centerChunkY = heightInChunks/2;
        centerBlockX = centerChunkX * chunkWidth;
        centerBlockY = centerChunkY * chunkHeight;
        
        foreach (var type in Block.types.Values) {
            foreach (var comp in type.GetComponents<BlockType>()) {
                blockTypeCache[comp.GetType()] = new List<Block>();
            }
        }
    }

    public bool IsCollisionEdge(IntVector2 bp) {
        var collisionLayer = CollisionLayer(bp);

        // we don't put colliders in empty space
        if (collisionLayer == Block.spaceLayer)
            return false;

        foreach (var neighbor in IntVector2.Neighbors(bp)) {
            if (CollisionLayer(neighbor) != collisionLayer)
                return true;
        }

        return false;
    }

    public int CollisionLayer(IntVector2 bp) {
		var baseBlock = this[bp, BlockLayer.Base];
		if (baseBlock == null)
			return Block.spaceLayer;

		var topBlock = this[bp, BlockLayer.Top];
        if (topBlock == null)
            return baseBlock.CollisionLayer;
        
        return Math.Max(baseBlock.CollisionLayer, topBlock.CollisionLayer);
    }

    public Block Topmost(IntVector2 bp) {
        var topBlock = this[bp, BlockLayer.Top];
        var baseBlock = this[bp, BlockLayer.Base];

        if (topBlock != null) 
            return topBlock;
        return baseBlock;
    }

	public IEnumerable<Block> Find(BlockType blockType) {
		foreach (var block in blockTypeCache[blockType.GetType()]) {
			yield return block;
		}
	}

    public IEnumerable<Block> Find<T>() {
        foreach (var block in blockTypeCache[typeof(T)]) {
            yield return block;
        }
    }

    public bool Has<T>() {
        return blockTypeCache[typeof(T)].Count > 0;
    }

    public bool Has(BlockType type) {
        return blockTypeCache[type.GetType()].Count > 0;
    }

    public IEnumerable<BlockChunk> AllChunks {
        get {
            for (var i = 0; i < widthInChunks; i++) {
                for (var j = 0; j < heightInChunks; j++) {
                    var chunk = baseChunks[i, j];
                    if (chunk != null) yield return chunk;
                    chunk = topChunks[i, j];
                    if (chunk != null) yield return chunk;
                }
            }
        }
    }

    public IEnumerable<Block> AllBlocks {
        get {
            var seenBlocks = new Dictionary<Block, Boolean>();
            foreach (var chunk in AllChunks) {
                foreach (var block in chunk.AllBlocks) {
                    if (!seenBlocks.ContainsKey(block)) {
                        yield return block;
                        seenBlocks[block] = true;
                    }
                }
            }
        }
    }

    public int Count {
        get {
            return AllBlocks.Count();
        }
    }

    public BlockChunk NewChunk(BlockChunk[,] chunks, int trueChunkX, int trueChunkY) {
        //Debug.LogFormat("{0} {1}", trueChunkX - centerChunkX, trueChunkY - centerChunkY);
        
        var chunk = new BlockChunk();
        chunks[trueChunkX, trueChunkY] = chunk;
        
        if (OnChunkCreated != null)
            OnChunkCreated(chunk);

        return chunk;
    }

    private void SetChunkedValue(int x, int y, BlockLayer layer, Block block) {
        BlockChunk[,] chunks;
        if (layer == BlockLayer.Base)
            chunks = baseChunks;
        else
            chunks = topChunks;

        var trueX = centerBlockX + x;
        var trueY = centerBlockY + y;
        var trueChunkX = trueX/chunkWidth;
        var trueChunkY = trueY/chunkHeight;
        var localX = trueX%chunkWidth;
        var localY = trueY%chunkHeight;

        var chunk = chunks[trueChunkX, trueChunkY];

        if (chunk == null) {
            chunk = NewChunk(chunks, trueChunkX, trueChunkY);
        }

        chunk[localX, localY] = block;

        if (x >= maxX || x <= minX || y >= maxY || y <= minY)
            RecalcBounds();
    }

    void RecalcBounds() {
        minX = 10000;
        minY = 10000;
		maxX = -10000;
        maxY = -10000;

        foreach (var block in AllBlocks) {
            minX = Math.Min(minX, block.pos.x);
            minY = Math.Min(minY, block.pos.y);
            maxX = Math.Max(maxX, block.pos.x);
            maxY = Math.Max(maxY, block.pos.y);
        }

		width = 1 + maxX - minX;
		height = 1 + maxY - minY;
		boundingRect = new Rect();
		boundingRect.xMin = minX;
		boundingRect.xMax = maxX;
		boundingRect.yMin = minY;
		boundingRect.yMax = maxY;
    }

    void RemoveBlock(Block block) {
        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                if (block.layer == BlockLayer.Base) size -= 1;
                SetChunkedValue(block.pos.x + i, block.pos.y + j, block.layer, null);
            }
        }

        if (block.gameObject != null)
            Pool.Recycle(block.gameObject);        

        block.ship = null;
        blockTypeCache[block.type.GetType()].Remove(block);
        if (OnBlockRemoved != null) OnBlockRemoved(block);        
    }

    void AssignBlock(Block block, IntVector2 bp, BlockLayer layer) {
        block.pos = bp;
		block.ship = ship;

        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                if (layer == BlockLayer.Base) size += 1;
                SetChunkedValue(bp.x + i, bp.y + j, layer, block);
            }
        }
    
        blockTypeCache[block.type.GetType()].Add(block);
        if (OnBlockAdded != null) OnBlockAdded(block);
    }

    public void RemoveSurface(IntVector2 bp) {
        var top = this[bp, BlockLayer.Top];
        if (top != null)
            RemoveBlock(top);
        else {
            var block = this[bp, BlockLayer.Base];
            if (block != null) RemoveBlock(block);
        }
    }

    public Block this[IntVector2 bp, BlockLayer layer] {
        get {
            BlockChunk[,] chunks;
            if (layer == BlockLayer.Base)
                chunks = baseChunks;
            else
                chunks = topChunks;

            var trueX = centerBlockX + bp.x;
            var trueY = centerBlockY + bp.y;
            var chunkX = trueX/chunkWidth;
            var chunkY = trueY/chunkHeight;
            var localX = trueX%chunkWidth;
            var localY = trueY%chunkHeight;

            if (chunkX < 0 || chunkX >= chunkWidth || chunkY < 0 || chunkY >= chunkHeight)
                return null;

            var chunk = chunks[chunkX, chunkY];
            if (chunk == null) return null;

            return chunk[localX, localY];
        }
        set {
            Profiler.BeginSample("BlockChunk[bp]=");

            if (value != null && value.ship != null)
                throw new ArgumentException("This block is already attached to a ship!");

            var width = 1;
            var height = 1;
            if (value != null) {
                width = value.Width;
                height = value.Height;
            }

            for (var i = 0; i < width; i++) {
                for (var j = 0; j < height; j++) {
                    var current = this[new IntVector2(bp.x + i, bp.y + j), layer];
                    if (current != null) RemoveBlock(current);
                }
            }

            if (value != null)
                AssignBlock(value, bp, layer);

            Profiler.EndSample();    
        }
    }

    public Block this[int x, int y, BlockLayer layer] {
        get { return this[new IntVector2(x, y), layer]; }
        set { this[new IntVector2(x, y), layer] = value; }
    }

    public IEnumerable<Block> this[IntVector2 bp] {        
        get {
            var baseBlock = this[bp, BlockLayer.Base];
            var topBlock = this[bp, BlockLayer.Top];
            if (baseBlock != null) yield return baseBlock;
            if (topBlock != null) yield return topBlock;
        }
    }

    public IEnumerable<Block> this[int x, int y] {
        get { return this[new IntVector2(x, y)]; }
    }

	public bool IsOutsideBounds(IntVector2 bp) {
		// There's a one-tile walkable area of open space around the ship
		return (bp.x > maxX+1 || bp.x < minX-1 || bp.y > maxY+1 || bp.y < minY-1);
	}

    public bool IsPassable(IntVector2 bp) {
		var collisionLayer = CollisionLayer(bp);
		if (collisionLayer == Block.floorLayer)
			return true;

		if (collisionLayer == Block.spaceLayer) {
			if (IsOutsideBounds(bp))
				return false;

			foreach (var neighbor in IntVector2.NeighborsWithDiagonal(bp)) {
				var secondLayer = CollisionLayer(neighbor);
				if (secondLayer != Block.spaceLayer)
					return true;
			}
		}

		return false;
    }
}
