using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BlockPather {
	// find all blocks directly connected to a given block
	public static IEnumerable<Block> Floodfill(BlockMap blocks, Block start) {
		var seen = new bool[blocks.width, blocks.height];
		var heads = new Stack<Block>();
		seen[start.x-blocks.minX, start.y-blocks.minY] = true;
		heads.Push(start);
		
		while (heads.Count > 0) {
			var head = heads.Pop();

			foreach (var neighbor in IntVector2.Neighbors(head.pos)) {
				var seenX = neighbor.x-blocks.minX;
				var seenY = neighbor.y-blocks.minY;

				if (seen[seenX, seenY]) continue;
				seen[seenX, seenY] = true;

				var nextBlock = blocks[neighbor, start.layer];
				if (nextBlock != null) {
					yield return nextBlock;
					heads.Push(nextBlock);
				}
			}
		}
	}

	public static IEnumerable<IntVector2> Floodwalk(BlockMap blocks, IntVector2 start) {
		var heads = new Stack<IntVector2>();
		var seen = new bool[blocks.width, blocks.height];
		seen[start.x-blocks.minX, start.y-blocks.minY] = true;
		heads.Push(start);

		while (heads.Count > 0) {
			var head = heads.Pop();

			foreach (var neighbor in IntVector2.Neighbors(head)) {
				var seenX = neighbor.x-blocks.minX;
				var seenY = neighbor.y-blocks.minY;
				if (seen[seenX, seenY] || !blocks.IsPassable(neighbor))
					continue;

				yield return neighbor;

				seen[seenX, seenY] = true;
				heads.Push(neighbor);
			}
		}
	}

	public static IEnumerable<IntVector2> Floodsight(BlockMap blocks, IntVector2 start) {
		var heads = new Stack<IntVector2>();
		var seen = new bool[blocks.width+2, blocks.height+2];
		seen[start.x-blocks.minX, start.y-blocks.minY] = true;
		heads.Push(start);

		while (heads.Count > 0) {
			var head = heads.Pop();
			
			foreach (var neighbor in IntVector2.NeighborsWithDiagonal(head)) {
				if (blocks.IsOutsideBounds(neighbor) || seen[neighbor.x-blocks.minX+1, neighbor.y-blocks.minY+1])
					continue;
				
				yield return neighbor;

				seen[neighbor.x-blocks.minX+1, neighbor.y-blocks.minY+1] = true;

				if (!blocks[neighbor].Any((b) => b.type.canBlockSight))
					heads.Push(neighbor);
			}
		}
	}


	public static bool PathExists(BlockMap blocks, IntVector2 start, IntVector2 end) {
		if (!blocks.IsPassable(end))
			return false;

		var heads = new List<IntVector2>() { start };
		var seen = new HashSet<IntVector2>();

		while (heads.Count > 0) {
			var head = heads[0];
			heads.RemoveAt(0);

			foreach (var neighbor in IntVector2.Neighbors(head)) {
				if (seen.Contains(neighbor) || !blocks.IsPassable(neighbor))
					continue;

				if (neighbor == end)
					return true;

				seen.Add(neighbor);
				heads.Add(neighbor);
			}
		}

		return false;
	}

	/*struct BlockWithNeighborCount {
		Block block;
		int count;

		public BlockWithNeighborCount(Block block, int count) {
			this.block = block;
			this.count = count;
		}
	}

	public static List<Rect> DivideIntoBoxes(BlockMap blocks) {
		foreach (var block in blocks.AllBlocks) {
			var ncount = IntVector2.NeighborsWithDiagonal(block.pos).Where((n) => block.
			var annoBlock = new BlockWithNeighborCount(block);
		}
	}*/

    public static List<IntVector2> PathBetween(BlockMap blocks, IntVector2 start, IntVector2 end) {
        //Debug.LogFormat("{0} {1} {2} {3}", minX, minY, maxX, maxY);
        // nodes that have already been analyzed and have a path from the start to them
        var closedSet = new HashSet<IntVector2>();
        // nodes that have been identified as a neighbor of an analyzed node, but have 
        // yet to be fully analyzed
        var openSet = new HashSet<IntVector2> { start };
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
			var current = openSet.OrderBy((n) => predictedDistance[n]).First();
            	
            // if it is the finish, return the path
            if (current.x == end.x && current.y == end.y) {
                // generate the found path
                return ReconstructPath(cameFrom, end);
            }
            
            // move current node from open to closed
            openSet.Remove(current);
            closedSet.Add(current);
            
            // process each valid node around the current node
            foreach (var neighbor in IntVector2.Neighbors(current)) {
                if (neighbor != start && !blocks.IsPassable(neighbor)) {
                    continue;
                }
                
                var tempCurrentDistance = currentDistance[current] + 1;

                // if we already know a faster way to this neighbor, use that route and 
                // ignore this one
                if (closedSet.Contains(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) {
                    continue;
                }
                
                // if we don't know a route to this neighbor, or if this is faster, 
                // store this route
                if (!closedSet.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor]) {
					cameFrom[neighbor] = current;
                    
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
    public static List<IntVector2> ReconstructPath(Dictionary<IntVector2, IntVector2> cameFrom, IntVector2 current) {
        if (!cameFrom.Keys.Contains(current)) {
            return new List<IntVector2> { current };
        }
        
        var path = ReconstructPath(cameFrom, cameFrom[current]);
        path.Add(current);
        return path;
    }
}
