﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BlockPather {
	public static List<IntVector2> PathBetween(BlockMap blocks, IntVector2 start, IntVector2 end) {
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
			foreach (var neighbor in IntVector2.Neighbors(current)) {
				if (neighbor.x > blocks.maxX+2 || neighbor.x < blocks.minX-2 || neighbor.y > blocks.maxY+2 || neighbor.y < blocks.minY-2 || !blocks.IsPassable(neighbor)) {
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
	public static List<IntVector2> ReconstructPath(Dictionary<IntVector2, IntVector2> cameFrom, IntVector2 current) {
		if (!cameFrom.Keys.Contains(current)) {
			return new List<IntVector2> { current };
		}
		
		var path = ReconstructPath(cameFrom, cameFrom[current]);
		path.Add(current);
		return path;
	}
}