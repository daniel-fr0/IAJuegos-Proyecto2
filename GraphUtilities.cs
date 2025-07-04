using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Node
{
	public int level;
	public Rect bounds;
	public Vector3 center;
	public Node representativeChild;
	public Node parent;
	// Tactical benefits/penalties if it is a tactical waypoint
	public float tacticalBenefit = 0;

	public Node(int level)
	{
		this.level = level;
	}

	public Node(Vector3 position)
	{
		// This is the constructor for the Node class for a level 0 node
		level = 0;

		// At level 0 is a tile node
		// The position floor is used to get the integer position of the tile
		// that corresponds to the bottom left corner of the tile
		int x = Mathf.FloorToInt(position.x);
		int y = Mathf.FloorToInt(position.y);

		// The center is the center of the tile
		center = new Vector3(x + 0.5f, y + 0.5f, 0);
	}

	public Vector3 GetPosition()
	{
		return center;
	}

	public bool Contains(Node other)
	{
		Vector3 leftBottom = new Vector3(bounds.xMin, bounds.yMin, 0) + GetPosition();
		Vector3 rightTop = new Vector3(bounds.xMax, bounds.yMax, 0) + GetPosition();

		Vector3 position = other.GetPosition();

		return position.x >= leftBottom.x && position.x <= rightTop.x &&
			position.y >= leftBottom.y && position.y <= rightTop.y;
	}

	public bool Contains(Vector3 position)
	{
		if (level == 0)
		{
			// A tile node has 1x1 bounds
			Vector3 leftBottom = new Vector3(center.x - 0.5f, center.y - 0.5f, 0);
			Vector3 rightTop = new Vector3(center.x + 0.5f, center.y + 0.5f, 0);

			return position.x >= leftBottom.x && position.x <= rightTop.x &&
				position.y >= leftBottom.y && position.y <= rightTop.y;
		}

		return Contains(new Node(position));
	}

    public override bool Equals(object obj)
    {
        // Compare the level and position of the HierarchicalNode
		Node node = obj as Node;
		if (node == null)
		{
			return false;
		}

		return level == node.level && GetPosition() == node.GetPosition();
    }

	public override int GetHashCode()
	{
		// Generate a unique hash code based on the level and position
		string hash = $"({level},{center.x},{center.y})";
		return hash.GetHashCode();
	}
}

public class Connection
{
	public Node fromNode;
	public Node toNode;
	public float cost;

	public Connection(Node fromNode, Node toNode, float cost)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
		this.cost = cost;
	}
}

public class Graph
{
	private Dictionary<Node, List<Connection>> connections = new Dictionary<Node, List<Connection>>();

	public int Length
	{
		get { return connections.Count; }
	}

	public Connection[] GetConnections(Node fromNode)
	{
		return connections[fromNode].ToArray();
	}

	public void AddConnection(Node fromNode, Node toNode, float cost = 1.0f)
	{
		Connection connection = new Connection(fromNode, toNode, cost);

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

	public bool ContainsNode(Node node)
	{
		return connections.ContainsKey(node);
	}

	public bool ContainsConnection(Node fromNode, Node toNode)
	{
		if (!ContainsNode(fromNode))
		{
			return false;
		}

		foreach (Connection connection in connections[fromNode])
		{
			if (connection.toNode.Equals(toNode))
			{
				return true;
			}
		}

		return false;
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
