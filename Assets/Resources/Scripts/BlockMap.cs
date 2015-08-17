using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BlockMap : MonoBehaviour {
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
	public BlockChunk[,] chunks;

	public Dictionary<Type, List<Block>> blockTypeCache;

	public delegate void BlockChangedHandler(Block newBlock, Block oldBlock);
	public event BlockChangedHandler OnBlockChanged;

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
		chunks = new BlockChunk[chunkWidth, chunkHeight];

		centerChunkX = widthInChunks/2;
		centerChunkY = heightInChunks/2;
		centerBlockX = centerChunkX * chunkWidth;
		centerBlockY = centerChunkY * chunkHeight;
		
		blockTypeCache = new Dictionary<Type, List<Block>>();
		foreach (var type in Block.types.Values) {
			foreach (var comp in type.GetComponents<BlockComponent>()) {
				blockTypeCache[comp.GetType()] = new List<Block>();
			}
		}
	} 

	public IntVector2[] Neighbors(IntVector2 bp) {
		return new IntVector2[] {
			new IntVector2(bp.x-1, bp.y),
			new IntVector2(bp.x+1, bp.y),
	 		new IntVector2(bp.x, bp.y-1),
			new IntVector2(bp.x, bp.y+1)
		};
	}

	public IntVector2[] NeighborsWithDiagonal(IntVector2 bp) {
		return new IntVector2[] {
			new IntVector2(bp.x-1, bp.y),
			new IntVector2(bp.x+1, bp.y),
			new IntVector2(bp.x, bp.y-1),
			new IntVector2(bp.x, bp.y+1),

			new IntVector2(bp.x-1, bp.y-1),
			new IntVector2(bp.x-1, bp.y+1),
			new IntVector2(bp.x+1, bp.y-1),
			new IntVector2(bp.x+1, bp.y+1)
		};
	}
	
	public bool IsEdge(IntVector2 bp) {
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
					var chunk = chunks[i, j];
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
		foreach (var chunk in AllChunks) {
			//chunk.renderer.enabled = true;
		}
	}
	
	public void DisableRendering() {
		foreach (var chunk in AllChunks) {
			//chunk.renderer.enabled = false;
		}
	}

	public Block this[IntVector2 bp] {
		get {
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
			var trueX = centerBlockX + bp.x;
			var trueY = centerBlockY + bp.y;
			var trueChunkX = trueX/chunkWidth;
			var trueChunkY = trueY/chunkHeight;
			var localX = trueX%chunkWidth;
			var localY = trueY%chunkHeight;

			var chunk = chunks[trueChunkX, trueChunkY];
			if (chunk == null) {
				//Debug.LogFormat("{0} {1}", trueChunkX - centerChunkX, trueChunkY - centerChunkY);

				chunk = Pool.For("BlockChunk").TakeObject().GetComponent<BlockChunk>();
				chunk.transform.parent = transform;
				chunk.transform.localPosition = new Vector2(
					(trueChunkX - centerChunkX) * chunkWidth * Tile.worldSize, 
					(trueChunkY - centerChunkY) * chunkHeight * Tile.worldSize
				);	
				//Debug.Log(chunk.transform.localPosition);
				chunk.gameObject.SetActive(true);
				chunks[trueChunkX, trueChunkY] = chunk;

				if (OnChunkCreated != null)
					OnChunkCreated(chunk);
			}

			var currentBlock = chunk[localX, localY];
			chunk[localX, localY] = value;
			if (value != null) {
				value.pos = bp;
			}

			if (value == null && currentBlock == null) {
				return;
			} else if (value == null && currentBlock != null) {
				// removing an existing block
				foreach (var comp in currentBlock.type.GetComponents<BlockComponent>()) {
					blockTypeCache[comp.GetType()].Remove(currentBlock);
				}
				OnBlockChanged(value, currentBlock);
			} else if (value != null && currentBlock == null) {
				// adding a new block
				foreach (var comp in value.type.GetComponents<BlockComponent>()) {
					blockTypeCache[comp.GetType()].Add(value);
				}
				OnBlockChanged(value, currentBlock);					
			} else if (value != null && currentBlock != null) {
				// replacing an existing block
				foreach (var comp in currentBlock.type.GetComponents<BlockComponent>()) {
					blockTypeCache[comp.GetType()].Remove(currentBlock);
				}
				foreach (var comp in value.type.GetComponents<BlockComponent>()) {
					blockTypeCache[comp.GetType()].Add(value);
				}

				OnBlockChanged(value, currentBlock);
			}
		}
	}

	public Block this[int x, int y] {
		get { return this[new IntVector2(x, y)]; }
		set { this[new IntVector2(x, y)] = value; }
	}

	public bool IsPassable(IntVector2 bp) {
		return (this[bp] == null || this[bp].CollisionLayer == Block.floorLayer);
	}

	public List<IntVector2> PathBetween(IntVector2 start, IntVector2 end) {
		//Debug.LogFormat("{0} {1} {2} {3}", minX, minY, maxX, maxY);
		// nodes that have already been analyzed and have a path from the start to them
		var closedSet = new List<IntVector2>();
		// nodes that have been identified as a neighbor of an analyzed node, but have 
		// yet to be fully analyzed
		var openSet = new List<IntVector2> { start };
		// a dictionary identifying the optimal origin Cell to each node. this is used 
		// to back-track from the end to find the optimal path
		var cameFrom = new Dictionary<IntVector2, IntVector2>();
		// a dictionary indicating how far each analyzed node is from the start
		var currentDistance = new Dictionary<IntVector2, int>();
		// a dictionary indicating how far it is expected to reach the end, if the path 
		// travels through the specified node. 
		var predictedDistance = new Dictionary<IntVector2, float>();
		
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
	public List<IntVector2> ReconstructPath(Dictionary<IntVector2, IntVector2> cameFrom, IntVector2 current) {
		if (!cameFrom.Keys.Contains(current)) {
			return new List<IntVector2> { current };
		}
		
		var path = ReconstructPath(cameFrom, cameFrom[current]);
		path.Add(current);
		return path;
	}
}
