using System.Collections.Generic;
using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
	public Graph graph;
	public HierarchicalGraph hierarchicalGraph;

	#region Singleton Initialization
	public static PathFinderManager instance;
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
	}
	#endregion

	public Connection[] PathFindAStar(Vector3 startPos, Vector3 endPos)
	{
		Node start = new Node(startPos);
		Node end = new Node(endPos);
		NodeRecord current = null;
		Heuristic heuristic = new ManhattanHeuristic(end);

		// Initialize the record for the start node
		NodeRecord startRecord = new NodeRecord(start, null, 0, heuristic.Estimate(start));

		// Initialize the open and closed lists
		PathFindingList open = new PathFindingList();
		open += startRecord;
		PathFindingList closed = new PathFindingList();

		// Iterate through processing each node
		while (open.Length() > 0)
		{
			// Find the smallest element in the open list (using the estimated total cost)
			current = open.SmallestElement();

			// If it is the goal node, then terminate
			if (current.node.Equals(end))
			{
				break;
			}

			// Otherwise get its outgoing connections
			Connection[] connections = graph.GetConnections(current.node);

			// Loop through each connection in turn
			foreach (Connection connection in connections)
			{
				// Get the cost estimate for the end node
				Node endNode = connection.toNode;
				float endNodeCost = current.costSoFar + connection.cost;
				float endNodeHeuristic;
				NodeRecord endNodeRecord = null;

				// If the node is closed, maybe skip or remove it from the closed list
				if (closed.Contains(endNode))
				{
					// Record in the closed list corresponding to the end node
					endNodeRecord = closed.Find(endNode);

					// If we didn't find a shorter route, skip
					if (endNodeRecord.costSoFar <= endNodeCost)
					{
						continue;
					}

					// Otherwise remove it from the closed list
					closed -= endNodeRecord;

					// We can use the node's old cost values to calculate its heuristic
					endNodeHeuristic = endNodeRecord.estimatedTotalCost - endNodeRecord.costSoFar;
				}
				// Skip if the node is open and we've not found a better route
				else if (open.Contains(endNode))
				{
					// Record in the open list corresponding to the end node
					endNodeRecord = open.Find(endNode);

					// If our route is no better, then skip
					if (endNodeRecord.costSoFar <= endNodeCost)
					{
						continue;
					}

					// We can use the node's old cost values to calculate its heuristic
					endNodeHeuristic = endNodeRecord.estimatedTotalCost - endNodeRecord.costSoFar;
				}
				// Otherwise, we know we've got an unvisited node, so make a record for it
				else
				{
					endNodeRecord = new NodeRecord(endNode, null, 0, 0);
					// We'll need to calculate the heuristic value using the function, since we don't have an existing record to use
					endNodeHeuristic = heuristic.Estimate(endNode);
				}

				// We're here if we need to update the node. Update the cost, estimate and connection
				endNodeRecord.costSoFar = endNodeCost;
				endNodeRecord.connection = connection;
				endNodeRecord.estimatedTotalCost = endNodeCost + endNodeHeuristic;

				// And add it to the open list
				if (!open.Contains(endNode))
				{
					open += endNodeRecord;
				}
			}

			// We've finished looking at the connections for the current node, so add it to the closed list, and remove it from the open list
			open -= current;
			closed += current;                
		}

		// We're here if we've either found the goal, or we've no more nodes to search, find which
		if (!current.node.Equals(end))
		{
			return null; // We've run out of nodes, and haven't found the goal
		}

		// At this point, we've got the path, but we need to return it in the correct order
		List<Connection> path = new List<Connection>();

		// Work back along the path, accumulating connections
		while (!current.node.Equals(start))
		{
			path.Add(current.connection);
			current = closed.Find(current.connection.fromNode);
		}
		
		// Reverse the path, and return it
		path.Reverse();
		return path.ToArray();
	}
}
