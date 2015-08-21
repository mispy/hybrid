using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BlockMap : PoolBehaviour {
	public int maxX;
	public int minX;
	public int maxY;
	public int minY;
	public int centerBlockX;
	public int centerBlockY;
	public int centerChunkX;
	public int centerChunkY;

	public int chunkWidth;
	public int chunkHeight;
	public int widthInChunks;
	public int heightInChunks;
	public BlockChunk[,] baseChunks;
	public BlockChunk[,] topChunks;
	public TileLayer baseTiles;
	public TileLayer topTiles;

	public Dictionary<Type, List<Block>> blockTypeCache = new Dictionary<Type, List<Block>>();
	public Dictionary<IntVector3, bool> edgeCache = new Dictionary<IntVector3, bool>();

	public delegate void BlockAddedHandler(Block newBlock);
	public delegate void BlockRemovedHandler(Block oldBlock);
	public delegate void BlockAddedOrRemovedHandler(Block block);
	public event BlockAddedHandler OnBlockAdded;
	public event BlockRemovedHandler OnBlockRemoved;

	public delegate void ChunkCreatedHandler(BlockChunk newChunk);
	public event ChunkCreatedHandler OnChunkCreated;

	public BlockMap() {
		minX = 0;
		minY = 0;
		maxX = 0;
		maxY = 0;

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

	public IntVector3[] Neighbors(IntVector3 bp) {
		return new IntVector3[] {
			new IntVector3(bp.x-1, bp.y),
			new IntVector3(bp.x+1, bp.y),
	 		new IntVector3(bp.x, bp.y-1),
			new IntVector3(bp.x, bp.y+1)
		};
	}

	public IntVector3[] NeighborsWithDiagonal(IntVector3 bp) {
		return new IntVector3[] {
			new IntVector3(bp.x-1, bp.y),
			new IntVector3(bp.x+1, bp.y),
			new IntVector3(bp.x, bp.y-1),
			new IntVector3(bp.x, bp.y+1),

			new IntVector3(bp.x-1, bp.y-1),
			new IntVector3(bp.x-1, bp.y+1),
			new IntVector3(bp.x+1, bp.y-1),
			new IntVector3(bp.x+1, bp.y+1)
		};
	}

	public bool IsEdge(IntVector3 bp) {
		Profiler.BeginSample("IsEdge");
		
		var ret = false;
		var block = this[bp];
		if (block != null) {
			foreach (var neighbor in Neighbors(bp)) {
				var other = this[neighbor];
				if (other == null || other.CollisionLayer != block.CollisionLayer) {
					ret = true;
				}
			}
		}
		
		Profiler.EndSample();
		return ret;
	}

	
	public bool IsCollisionEdge(IntVector3 bp) {
		Profiler.BeginSample("IsEdge");

		var ret = false;
		var block = this[bp];
		if (block != null) {
			foreach (var neighbor in Neighbors(bp)) {
				var other = this[neighbor];
				if (other == null || other.CollisionLayer != block.CollisionLayer) {
					ret = true;
				}
			}
		}

		Profiler.EndSample();
		return ret;
	}

	public IEnumerable<Block> Find<T>() {
		foreach (var block in blockTypeCache[typeof(T)]) {
			yield return block;
		}
	}

	public bool Has<T>() {
		return blockTypeCache[typeof(T)].Count > 0;
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
			foreach (var chunk in AllChunks) {
				foreach (var block in chunk.AllBlocks) {
					yield return block;
				}
			}
		}
	}

	public int Count {
		get {
			return AllBlocks.Count();
		}
	}
	
	public void EnableRendering() {
		baseTiles.EnableRendering();
		topTiles.EnableRendering();
		foreach (var renderer in GetComponentsInChildren<SpriteRenderer>()) {
			renderer.enabled = true;
		}
	}
	
	public void DisableRendering() {
		baseTiles.DisableRendering();
		topTiles.DisableRendering();
		foreach (var renderer in GetComponentsInChildren<SpriteRenderer>()) {
			renderer.enabled = false;
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

	private void SetChunkedValue(int x, int y, int layer, Block block) {
		BlockChunk[,] chunks = baseChunks;
		if (layer == Block.topLayer)
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

		var bp = new IntVector3(x, y, Block.baseLayer);
		edgeCache[bp] = IsEdge(bp);
		foreach (var pos in Neighbors(bp)) {
			edgeCache[pos] = IsEdge(pos);
		}
	}

	private void SetTileValue(int x, int y, int layer, Tile tile) {
		var tileLayer = baseTiles;
		if (layer == Block.topLayer)
			tileLayer = topTiles;

		tileLayer[x, y] = tile;
	}

	public void RemoveBlock(Block block) {
		for (var i = 0; i < block.Width; i++) {
			for (var j = 0; j < block.Height; j++) {
				SetChunkedValue(block.pos.x + i, block.pos.y + j, block.pos.z, null);
				SetTileValue(block.pos.x + i, block.pos.y + j, block.pos.z, null);
			}
		}

		if (block.gameObject != null)
			Pool.Recycle(block.gameObject);		

		block.ship = null;
		blockTypeCache[block.type.GetType()].Remove(block);
		if (OnBlockRemoved != null) OnBlockRemoved(block);		
	}

	public void AssignBlock(Block block, IntVector3 bp) {
		block.pos = bp;

		for (var i = 0; i < block.Width; i++) {
			for (var j = 0; j < block.Height; j++) {
				SetChunkedValue(bp.x + i, bp.y + j, bp.z, block);
				SetTileValue(bp.x + i, bp.y + j, bp.z, block.type.tileable.GetRotatedTile(i, j, block.orientation));
			}
		}
	
		blockTypeCache[block.type.GetType()].Add(block);
		if (OnBlockAdded != null) OnBlockAdded(block);
	}

	public void RemoveTop(IntVector3 bp) {
		var top = this[bp.x, bp.y, Block.topLayer];
		if (top != null)
			RemoveBlock(top);
		else {
			var block = this[bp.x, bp.y, Block.baseLayer];
			if (block != null) RemoveBlock(block);
		}
	}

	public Block this[IntVector3 bp] {
		get {
			BlockChunk[,] chunks = topChunks;
			if (bp.z == Block.baseLayer)
				chunks = baseChunks;

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
					var current = this[new IntVector3(bp.x + i, bp.y + j, bp.z)];
					if (current != null) RemoveBlock(current);
				}
			}

			if (value != null)
				AssignBlock(value, bp);

			Profiler.EndSample();	
		}
	}

	public Block this[int x, int y] {
		get { return this[new IntVector3(x, y, Block.baseLayer)]; }
		set { this[new IntVector3(x, y, Block.baseLayer)] = value; }
	}
	public Block this[int x, int y, int layer] {
		get { return this[new IntVector3(x, y, layer)]; }
		set { this[new IntVector3(x, y, layer)] = value; }
	}

	public bool IsPassable(IntVector3 bp) {
		return (this[bp] == null || this[bp].CollisionLayer == Block.floorLayer);
	}

	public List<IntVector3> PathBetween(IntVector3 start, IntVector3 end) {
		//Debug.LogFormat("{0} {1} {2} {3}", minX, minY, maxX, maxY);
		// nodes that have already been analyzed and have a path from the start to them
		var closedSet = new List<IntVector3>();
		// nodes that have been identified as a neighbor of an analyzed node, but have 
		// yet to be fully analyzed
		var openSet = new List<IntVector3> { start };
		// a dictionary identifying the optimal origin Cell to each node. this is used 
		// to back-track from the end to find the optimal path
		var cameFrom = new Dictionary<IntVector3, IntVector3>();
		// a dictionary indicating how far each analyzed node is from the start
		var currentDistance = new Dictionary<IntVector3, int>();
		// a dictionary indicating how far it is expected to reach the end, if the path 
		// travels through the specified node. 
		var predictedDistance = new Dictionary<IntVector3, float>();
		
		// initialize the start node as having a distance of 0, and an estmated distance 
		// of y-distance + x-distance, which is the optimal path in a square grid that 
		// doesn't allow for diagonal movement
		currentDistance.Add(start, 0);
		predictedDistance.Add(
			start,
			0 + +Math.Abs(start.x - end.x) + Math.Abs(start.x - end.x)
			);
		
		// if there are any unanalyzed nodes, process them
		while (openSet.Count > 0) {
			// get the node with the lowest estimated cost to finish
			
			var current = (
				from p in openSet orderby predictedDistance[p] ascending select p
				).First();
			
			// if it is the finish, return the path
			if (current.x == end.x && current.y == end.y) {
				// generate the found path
				return ReconstructPath(cameFrom, end);
			}
			
			// move current node from open to closed
			openSet.Remove(current);
			closedSet.Add(current);
			
			// process each valid node around the current node
			foreach (var neighbor in Neighbors(current)) {
				if (neighbor.x > maxX+2 || neighbor.x < minX-2 || neighbor.y > maxY+2 || neighbor.y < minY-2 || !IsPassable(neighbor)) {
					continue;
				}
				
				var tempCurrentDistance = currentDistance[current] + 1;
				
				// if we already know a faster way to this neighbor, use that route and 
				// ignore this one
				if (closedSet.Contains(neighbor)
				    && tempCurrentDistance >= currentDistance[neighbor]) {
					continue;
				}
				
				// if we don't know a route to this neighbor, or if this is faster, 
				// store this route
				if (!closedSet.Contains(neighbor)
				    || tempCurrentDistance < currentDistance[neighbor]) {
					if (cameFrom.Keys.Contains(neighbor)) {
						cameFrom[neighbor] = current;
					}
					else {
						cameFrom.Add(neighbor, current);
					}
					
					currentDistance[neighbor] = tempCurrentDistance;
					predictedDistance[neighbor] =
						currentDistance[neighbor]
						+ Math.Abs(neighbor.x - end.x)
							+ Math.Abs(neighbor.y - end.y);
					
					// if this is a new node, add it to processing
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					}
				}
			}
		}
		
		// unable to figure out a path, abort.
		return null;
	}

	/// <summary>
	/// Process a list of valid paths generated by the Pathfind function and return 
	/// a coherent path to current.
	/// </summary>
	/// <param name="cameFrom">A list of nodes and the origin to that node.</param>
	/// <param name="current">The destination node being sought out.</param>
	/// <returns>The shortest path from the start to the destination node.</returns>
	public List<IntVector3> ReconstructPath(Dictionary<IntVector3, IntVector3> cameFrom, IntVector3 current) {
		if (!cameFrom.Keys.Contains(current)) {
			return new List<IntVector3> { current };
		}
		
		var path = ReconstructPath(cameFrom, cameFrom[current]);
		path.Add(current);
		return path;
	}
}
