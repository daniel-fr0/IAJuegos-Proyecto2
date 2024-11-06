using UnityEngine;
using System;
using System.Collections.Generic;

public class Node
{
	public int row;
	public int column;

	public Node(Vector3 position)
	{
		// Get the row and column values from the position
		// this will identify the node in the grid
		row = Mathf.FloorToInt(position.x);
		column = Mathf.FloorToInt(position.y);
	}

	public Vector3 GetPosition()
	{
		// Return the position in the center of the node
		return new Vector3(row + 0.5f, column + 0.5f, 0.0f);
	}

	// Override Equals to compare two Node objects by their row and column values
	public override bool Equals(object obj)
	{
		Node node = obj as Node;
		if (node == null)
		{
			return false;
		}

		return row == node.row && column == node.column;
	}

	// Override GetHashCode to return a unique hash code for each Node object
	public override int GetHashCode()
	{
		// Use the row and column values to generate a unique hash code
		string hash = $"({row},{column})";
		return hash.GetHashCode();
	}
}

public class Connection
{
	public Node fromNode;
	public Node toNode;

	public Connection(Node fromNode, Node toNode)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
	}

	public float GetCost()
	{
		return 1.0f;
	}
}

public class Graph
{
	private Dictionary<Node, List<Connection>> connections = new Dictionary<Node, List<Connection>>();

	public Connection[] GetConnections(Node fromNode)
	{
		return connections[fromNode].ToArray();
	}

	public void AddConnection(Node fromNode, Node toNode)
	{
		Connection connection = new Connection(fromNode, toNode);

		if (!connections.ContainsKey(fromNode))
		{
			connections[fromNode] = new List<Connection>();
		}

		connections[fromNode].Add(connection);
	}

	public Node[] GetNodes()
	{
		return new List<Node>(connections.Keys).ToArray();
	}
}

public class NodeRecord : IComparer<NodeRecord>, IComparable<NodeRecord>
{
	public Node node;
	public Connection connection;
	public float costSoFar;
	public float estimatedTotalCost;

	public NodeRecord(Node node, Connection connection, float costSoFar, float estimatedTotalCost)
	{
		this.node = node;
		this.connection = connection;
		this.costSoFar = costSoFar;
		this.estimatedTotalCost = estimatedTotalCost;
	}

	public int Compare(NodeRecord x, NodeRecord y)
	{
		return x.costSoFar.CompareTo(y.costSoFar);
	}

	public int CompareTo(NodeRecord other)
	{
		return costSoFar.CompareTo(other.costSoFar);
	}
}

[Serializable]
public class NodeGroup
{
	public Vector2 fromNode;
	public Vector2 toNode;
}

[Serializable]
public class GraphLevel
{
	public NodeGroup[] groups;
}