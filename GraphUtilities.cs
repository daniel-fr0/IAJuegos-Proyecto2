using UnityEngine;
using System;
using System.Collections.Generic;

namespace World {
	class Node
	{
		public static int WIDTH = 10;
		public static int HEIGHT = 10;
		public int row;
		public int column;

		public Node(Vector2 position)
		{
			row = (int)position.x / HEIGHT + 1;
			column = (int)position.y / WIDTH + 1;
		}

		public Vector2 GetPosition()
		{
			return new Vector2(row * HEIGHT + HEIGHT/2.0f, column * WIDTH + WIDTH/2.0f);
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

	class Connection
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

	class Graph
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
	}

	class NodeRecord : IComparer<NodeRecord>, IComparable<NodeRecord>
	{
		public Node node;
		public Connection connection;
		public float costSoFar;

		public NodeRecord(Node node, Connection connection, float costSoFar)
		{
			this.node = node;
			this.connection = connection;
			this.costSoFar = costSoFar;
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
}