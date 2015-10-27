using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;


[Serializable]
public class BlockData {
    public IntVector2 pos;
    public BlockType type;
    public Facing facing;
    public BlockLayer layer;
}

public class BlockMap : PoolBehaviour, ISerializationCallbackReceiver {
    [HideInInspector]
	public Ship ship;

	// Cached info
    [ReadOnly]
    public int maxX;
    [ReadOnly]
    public int minX;
    [ReadOnly]
    public int maxY;
    [ReadOnly]
    public int minY;
    [ReadOnly]
    public int width;
    [ReadOnly]
    public int height;
    [ReadOnly]
    public int baseSize;
    [NonSerialized]
    public Rect boundingRect = new Rect();
    [NonSerialized]
	public HashSet<Block> allBlocks = new HashSet<Block>();



	// Values used for translating between 0,0 center to traditional
	// array coordinates
    [SerializeField]
    [HideInInspector]
	int centerBlockX;
    [SerializeField]
    [HideInInspector]
    int centerBlockY;
    [SerializeField]
    [HideInInspector]
    int centerChunkX;
    [SerializeField]
    [HideInInspector]
    int centerChunkY;
    [SerializeField]
    [HideInInspector]
    int chunkWidth;
    [SerializeField]
    [HideInInspector]
    int chunkHeight;
    [SerializeField]
    [HideInInspector]
    int widthInChunks;
    [SerializeField]
    [HideInInspector]
    int heightInChunks;

    BlockChunk[,] baseChunks;
    BlockChunk[,] topChunks;

    [NonSerialized]
    public Dictionary<BlockType, HashSet<Block>> blockTypeCache = new Dictionary<BlockType, HashSet<Block>>();
    [NonSerialized]
    public Dictionary<Type, HashSet<Block>> blockCompCache = new Dictionary<Type, HashSet<Block>>();
    [NonSerialized]
    public HashSet<Block> frontBlockers = new HashSet<Block>();

    public delegate void BlockAddedHandler(Block newBlock);
    public delegate void BlockRemovedHandler(Block oldBlock);
    public event BlockAddedHandler OnBlockAdded;
    public event BlockRemovedHandler OnBlockRemoved;

    public delegate void ChunkCreatedHandler(BlockChunk newChunk);
    public event ChunkCreatedHandler OnChunkCreated;

    public override void OnCreate() {
        minX = 0;
        minY = 0;
        maxX = 0;
        maxY = 0;
		width = 0;
		height = 0;
		baseSize = 0;
        boundingRect = new Rect();
        allBlocks = new HashSet<Block>();

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
    }

    void OnEnable() {
        if (blockData.Count == 0) return;

        OnCreate();

        foreach (var data in blockData) {
            var block = new Block(BlockType.FromId(data.type.id));
            block.facing = data.facing;
            this[data.pos, data.layer] = block;
        }

        blockData.Clear();
    }

    public List<BlockData> blockData = new List<BlockData>();

    public void OnBeforeSerialize() {
        blockData = new List<BlockData>();

        foreach (var block in allBlocks) {
            var data = new BlockData();
            data.pos = block.pos;
            data.type = block.type;
            data.facing = block.facing;
            data.layer = block.layer;
            blockData.Add(data);
        }

        //Debug.Assert(blockData.Count > 0, "Expected blockData.Count > 0");
    }

    public void OnAfterDeserialize() {


        //Debug.Assert(blockData.Count > 0, "Expected blockData.Count > 0");
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

    public bool CanSeeThrough(IntVector2 bp) {
        foreach (var block in BlocksAtPos(bp)) {
            if (block.type.canBlockSight)
                return false;
        }

        return true;
    }

    public Block Topmost(IntVector2 bp) {
        var topBlock = this[bp, BlockLayer.Top];
        var baseBlock = this[bp, BlockLayer.Base];

        if (topBlock != null) 
            return topBlock;
        return baseBlock;
    }

    public IEnumerable<Block> Find(BlockType type) {
        if (!blockTypeCache.ContainsKey(type))
            yield break;

        foreach (var block in blockTypeCache[type])
            yield return block;
    }

    public IEnumerable<Block> Find(string typeName) {
        return Find(BlockType.FromId(typeName));
    }

    public IEnumerable<Block> Find<T>() {
        if (!blockCompCache.ContainsKey(typeof(T)))
            yield break;

        foreach (var block in blockCompCache[typeof(T)]) {
            yield return block;
        }
    }

    public bool Has<T>() {
        return Find<T>().Count() > 0;
    }

	public IEnumerable<IntVector2> FilledPositions {
		get {
			for (var i = minX; i <= maxX; i++) {
				for (var j = minY; j <= maxY; j++) {
					if (this[i, j, BlockLayer.Base] != null)
						yield return new IntVector2(i, j);
				}
			}
		}
	}

    /*public IEnumerable<Block> BaseBlocks {
        get {
			for (var i = 0; i < widthInChunks; i++) {
				for (var j = 0; j < heightInChunks; j++) {
					var chunk = baseChunks[i, j];
					foreach (var block in chunk.AllBlocks) {
						yield return block;
					}
				}
			}
		}
    }*/

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

        if (block == null) {
            CheckForShrink(x, y);
        } else {
            if (baseSize == 0) {
                // first block!
                maxX = x;
                maxY = y;
                minX = x;
                minY = y;
            }  else {
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
            }
        }
        
        width = 1 + maxX - minX;
        height = 1 + maxY - minY;
        boundingRect.xMin = minX;
        boundingRect.xMax = maxX;
        boundingRect.yMin = minY;
        boundingRect.yMax = maxY;
    }

    void CheckForShrink(int x, int y) {
        if (x == minX) {
            var shrink = true;
            for (var j = minY; j <= maxY; j++)
                if (this[minX, j, BlockLayer.Base] != null)
                    shrink = false;
            if (shrink) minX += 1;
        }

        if (x == maxX) {
            var shrink = true;
            for (var j = minY; j <= maxY; j++)
                if (this[maxX, j, BlockLayer.Base] != null)
                    shrink = false;
            if (shrink) maxX -= 1;
        }

        if (y == minY) {
            var shrink = true;
            for (var i = minX; i <= maxX; i++)
                if (this[i, minY, BlockLayer.Base] != null)
                    shrink = false;
            if (shrink) minY += 1;
        }
        
        if (y == maxY) {
            var shrink = true;
            for (var i = minX; i <= maxX; i++)
                if (this[i, maxY, BlockLayer.Base] != null)
                    shrink = false;
            if (shrink) maxY -= 1;
        }
        /*        minX = 10000;
        minY = 10000;
		maxX = -10000;
        maxY = -10000;

        foreach (var block in allBlocks) {
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
		boundingRect.yMax = maxY;*/
    }

    void RemoveBlock(Block block) {
        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                SetChunkedValue(block.pos.x + i, block.pos.y + j, block.layer, null);
                if (block.layer == BlockLayer.Base) baseSize -= 1;
            }
        }

        block.ship = null;
        blockTypeCache[block.type].Remove(block);
        foreach (var comp in block.type.blockComponents) {
            blockCompCache[comp.GetType()].Remove(block);
        }
		allBlocks.Remove(block);
        if (block.type.canBlockFront)
            frontBlockers.Remove(block);

        if (OnBlockRemoved != null) OnBlockRemoved(block);        
    }

    void AssignBlock(Block block, IntVector2 bp, BlockLayer layer) {
        block.pos = bp;
		block.ship = ship;

        for (var i = 0; i < block.Width; i++) {
            for (var j = 0; j < block.Height; j++) {
                SetChunkedValue(bp.x + i, bp.y + j, layer, block);
                if (layer == BlockLayer.Base) baseSize += 1;
            }
        }
    
        if (!blockTypeCache.ContainsKey(block.type))
            blockTypeCache[block.type] = new HashSet<Block>();
        blockTypeCache[block.type].Add(block);
        foreach (var comp in block.type.blockComponents) {
            if (!blockCompCache.ContainsKey(comp.GetType()))
                blockCompCache[comp.GetType()] = new HashSet<Block>();
            blockCompCache[comp.GetType()].Add(block);
        }

		allBlocks.Add(block);

        if (block.type.canBlockFront)
            frontBlockers.Add(block);

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
                    var current = this[bp.x + i, bp.y + j, layer];
                    if (current != null) RemoveBlock(current);

					// Remove any blocks above this
					if (value == null && layer == BlockLayer.Base) {
						current = this[bp.x + i, bp.y + j, BlockLayer.Top];
						if (current != null) RemoveBlock(current);
					}
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

    public IEnumerable<Block> BlocksAtPos(IntVector2 bp) {        
        var baseBlock = this[bp, BlockLayer.Base];
        var topBlock = this[bp, BlockLayer.Top];
        if (baseBlock != null) yield return baseBlock;
        if (topBlock != null) yield return topBlock;
    }

	public bool IsOutsideBounds(IntVector2 bp, bool allowBuffer = true) {
		if (allowBuffer) {
			// There's a one-tile walkable area of open space around the ship
			return (bp.x > maxX+1 || bp.x < minX-1 || bp.y > maxY+1 || bp.y < minY-1);
		} else {
			return (bp.x > maxX || bp.x < minX || bp.y > maxY || bp.y < minY);
		}
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
