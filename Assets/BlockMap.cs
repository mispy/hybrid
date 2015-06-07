using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class BlockMap : ISerializationCallbackReceiver {
	private Dictionary<IntVector2, Block> blockPositions;
	private Block[] blockSequence;
	[NonSerialized]
	public Vector3[] meshVertices;
	[NonSerialized]
	public Vector2[] meshUV;
	[NonSerialized]
	public int[] meshTriangles;

	[NonSerialized]
	public int maxX;
	[NonSerialized]
	public int minX;
	[NonSerialized]
	public int maxY;
	[NonSerialized]
	public int minY;

	public Dictionary<string, List<Block>> blockTypeCache = new Dictionary<string, List<Block>>();

	public delegate void OnBlockChangedDelegate(Block newBlock, Block oldBlock);
	public OnBlockChangedDelegate OnBlockChanged;

	private void BlockChangeEvent(Block newBlock, Block oldBlock) {
		if (OnBlockChanged != null) {
			OnBlockChanged(newBlock, oldBlock);
		}
	}

	[SerializeField]
	public BlockData[] saveData;
	
	public void OnBeforeSerialize() {
		saveData = new BlockData[this.Count];
		var allBlocks = this.All.ToArray();

		for (var i = 0; i < allBlocks.Length; i++) {
			saveData[i] = allBlocks[i].Serialize();
		}
	}

	public void OnAfterDeserialize() {	
		Debug.Log(saveData.Length);
	}

	public BlockMap() {
		blockPositions = new Dictionary<IntVector2, Block>(); 
		blockSequence = new Block[1];
		meshVertices = new Vector3[1*4];
		meshUV = new Vector2[1*4];
		meshTriangles = new int[1*6];

		minX = 0;
		minY = 0;
		maxX = 0;
		maxY = 0;
	} 

	public IntVector2[] Neighbors(IntVector2 bp) {
		return new IntVector2[] {
			new IntVector2(bp.x-1, bp.y),
			new IntVector2(bp.x+1, bp.y),
	 		new IntVector2(bp.x, bp.y-1),
			new IntVector2(bp.x, bp.y+1)
		};
	}
	
	public bool IsEdge(IntVector2 bp) {
		Profiler.BeginSample("IsEdge");

		var ret = false;
		var block = this[bp];
		if (block != null) {
			foreach (var neighbor in Neighbors(bp)) {
				var other = this[neighbor];
				if (other == null || other.collisionLayer != block.collisionLayer) {
					ret = true;
				}
			}
		}

		Profiler.EndSample();
		return ret;
	}

	public IEnumerable<Block> All {
		get {
			return blockPositions.Values;
		}
	}

	public int Count {
		get {
			return blockPositions.Count;
		}
	}

	public IEnumerable<Vector3> GetVertices(int x, int y) {
		var lx = x * Block.worldSize;
		var ly = y * Block.worldSize;
		yield return new Vector3(lx - Block.worldSize / 2, ly + Block.worldSize / 2, 0);
		yield return new Vector3(lx + Block.worldSize / 2, ly + Block.worldSize / 2, 0);
		yield return new Vector3(lx + Block.worldSize / 2, ly - Block.worldSize / 2, 0);
		yield return new Vector3(lx - Block.worldSize / 2, ly - Block.worldSize / 2, 0);
	/*meshVertices.Add(new Vector3(lx, ly, 0));
				meshVertices.Add(new Vector3(lx + Block.worldSize, ly, 0));
				meshVertices.Add(new Vector3(lx + Block.worldSize, ly - Block.worldSize, 0));
				meshVertices.Add(new Vector3(lx, ly - Block.worldSize, 0));*/
	
	}

	public void AttachToMesh(Block block) {		
		var i = block.index;
		blockSequence[i] = block;

		meshTriangles[i*6] = i*4;
		meshTriangles[i*6+1] = (i*4)+1;
		meshTriangles[i*6+2] = (i*4)+3;
		meshTriangles[i*6+3] = (i*4)+1;
		meshTriangles[i*6+4] = (i*4)+2;
		meshTriangles[i*6+5] = (i*4)+3;
		
		var verts = GetVertices(block.pos.x, block.pos.y).ToList();
		meshVertices[i*4] = verts[0];
		meshVertices[i*4+1] = verts[1];
		meshVertices[i*4+2] = verts[2];
		meshVertices[i*4+3] = verts[3];
		
		var uvs = Block.GetUVs(block);
		meshUV[i*4] = uvs[0];
		meshUV[i*4+1] = uvs[1];
		meshUV[i*4+2] = uvs[2];
		meshUV[i*4+3] = uvs[3];
	}

	public void ClearMeshPos(int i) {
		meshVertices[i*4] = Vector3.zero;
		meshVertices[i*4+1] = Vector3.zero;
		meshVertices[i*4+2] = Vector3.zero;
		meshVertices[i*4+3] = Vector3.zero;
	}

	public void ExpandBlockSequence(int newSize=-1) {
		if (newSize == -1)
			newSize = blockSequence.Length << 1;

		//Debug.LogFormat("Expanding to {0}", newSize);

		var newSequence = new Block[newSize];
		var newTriangles = new int[newSize*6];
		var newVertices = new Vector3[newSize*4];
		var newUV = new Vector2[newSize*4];

		for (var i = 0; i < blockSequence.Length; i++) {
			newSequence[i] = blockSequence[i];
		}

		for (var i = 0; i < meshTriangles.Length; i++) {
			newTriangles[i] = meshTriangles[i];
		}

		for (var i = 0; i < meshVertices.Length; i++) {
			newVertices[i] = meshVertices[i];
			newUV[i] = meshUV[i];
		}
	
		blockSequence = newSequence;
		meshTriangles = newTriangles;
		meshVertices = newVertices;
		meshUV = newUV;
	}

	public Block FindType(string typeName) {
		if (!blockTypeCache.ContainsKey(typeName))
			return null;

		foreach (var block in blockTypeCache[typeName]) {
			return block;
		}

		return null;
	}

	public bool HasType(string typeName) {
		return blockTypeCache.ContainsKey(typeName) && blockTypeCache[typeName].Count > 0;
	}

	public Block this[IntVector2 bp] {
		get {
			Profiler.BeginSample("blocks[]");
			Block ret = null;
			blockPositions.TryGetValue(bp, out ret);
			Profiler.EndSample();
			return ret;
		}
		set {
			var currentBlock = this[bp];

			if (value != null) {
				value.pos = bp;
				blockPositions[bp] = value; 

				if (!blockTypeCache.ContainsKey(value.typeName)) {
					blockTypeCache[value.typeName] = new List<Block>();
				}

				if (bp.x > maxX)
					maxX = bp.x;
				if (bp.y > maxY)
					maxY = bp.y;
				if (bp.x < minX)
					minX = bp.x;
				if (bp.y < minY)
					minY = bp.y;
			} else {
				blockPositions.Remove(bp);
			}


			if (value == null && currentBlock == null) {
				return;
			} else if (value == null && currentBlock != null) {
				// removing an existing block
				ClearMeshPos(currentBlock.index);
				blockTypeCache[currentBlock.typeName].Remove(currentBlock);
				BlockChangeEvent(value, currentBlock);
			} else if (value != null && currentBlock == null) {
				// adding a new block
				while (true) {
					for (var i = 0; i < blockSequence.Length; i++) {
						if (blockSequence[i] == null) {
							value.index = i;
							AttachToMesh(value);
							blockTypeCache[value.typeName].Add(value);
							BlockChangeEvent(value, currentBlock);					
							return;
						}
					}

					ExpandBlockSequence();
				}
			} else if (value != null && currentBlock != null) {
				// replacing an existing block
				value.index = currentBlock.index;
				AttachToMesh(value);
					
				blockTypeCache[currentBlock.typeName].Remove(currentBlock);
				blockTypeCache[value.typeName].Add(value);
				BlockChangeEvent(value, currentBlock);
			}

		}
	}

	public Block this[int x, int y] {
		get { return this[new IntVector2(x, y)]; }
		set { this[new IntVector2(x, y)] = value; }
	}

	public bool IsPassable(IntVector2 bp) {
		return (this[bp] == null || this[bp].collisionLayer == Block.floorLayer);
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
