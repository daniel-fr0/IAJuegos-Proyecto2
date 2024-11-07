using UnityEngine;
using System.Collections.Generic;


public class HierarchicalGraph
{
	public List<Graph> levels;

	public HierarchicalGraph()
	{
		levels = new List<Graph>();
	}

	public int Height()
	{
		return levels.Count;
	}

	// Get the node from the specified level given a node from the graph
	public Node GetNode(int level, Node node)
	{
		// Cannot get a node from a level that does not exist
		if (node.level >= levels.Count || level >= levels.Count || level < 0 || node.level < 0 || node == null)
		{
			return null;
		}


		// If the level is lower, look for the representative child
		if (level < node.level)
		{
			return GetNode(level, node.representativeChild);
		}

		// If the level is higher, look for the parent
		if (level > node.level)
		{
			return GetNode(level, node.parent);
		}

		// If the level is the same, return the node
		return node;
	}

	public Connection[] GetConnections(Node node)
	{
		return levels[node.level].GetConnections(node);
	}

	// Add a connection between two nodes in the hierarchical graph
	public void AddConnection(int level, Node fromNode, Node toNode)
	{
		// If is the first connection in the level, create a new graph
		if (levels.Count <= level)
		{
			levels.Add(new Graph());
		}

		// If it's a level 0 connection, add the connection as is
		if (level == 0)
		{
			levels[level].AddConnection(fromNode, toNode);
			levels[level].AddConnection(toNode, fromNode);
			return;
		}

		// Calculate the cost of the connection as the average of the children
		List<Node> fromChildren = new List<Node>();
		List<Node> toChildren = new List<Node>();

		// Find the representative children of the fromNode and toNode
		Node fromRep = null;
		Node toRep = null;
		float fromDist = float.MaxValue;
		float toDist = float.MaxValue;

		// Find the children of the fromNode and toNode
		foreach (Node lowerNode in levels[level-1].GetNodes())
		{
			// If the lower node is contained in the fromNode, set the parent
			if (fromNode.Contains(lowerNode.GetPosition()))
			{
				lowerNode.parent = fromNode;
				fromChildren.Add(lowerNode);

				// Find the representative child of the fromNode
				float dist = Vector3.Distance(fromNode.GetPosition(), lowerNode.GetPosition());
				if (dist < fromDist)
				{
					fromRep = lowerNode;
					fromDist = dist;
				}
			}
			// If the lower node is contained in the toNode, set the parent
			else if (toNode.Contains(lowerNode.GetPosition()))
			{
				lowerNode.parent = toNode;
				toChildren.Add(lowerNode);

				// Find the representative child of the toNode
				float dist = Vector3.Distance(toNode.GetPosition(), lowerNode.GetPosition());
				if (dist < toDist)
				{
					toRep = lowerNode;
					toDist = dist;
				}
			}
		}

		// Add the representative children to the fromNode and toNode
		fromNode.representativeChild = fromRep;
		toNode.representativeChild = toRep;

		// Calculate the cost of the connection as the average of costs of the children
		float cost = 0;
		foreach (Node fromChild in fromChildren)
		{
			foreach (Node toChild in toChildren)
			{
				// Cost is calculated as the manhattan distance between the children
				Vector3 fromPos = fromChild.GetPosition();
				Vector3 toPos = toChild.GetPosition();
				cost += Mathf.Abs(fromPos.x - toPos.x) + Mathf.Abs(fromPos.y - toPos.y);
			}
		}
		cost /= fromChildren.Count * toChildren.Count;

		// Add the connection to the level
		levels[level].AddConnection(fromNode, toNode, cost);
		levels[level].AddConnection(toNode, fromNode, cost);
	}
}