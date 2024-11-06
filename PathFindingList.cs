using Priority_Queue;


public class PathFindingList
{
	private SimplePriorityQueue<NodeRecord> queue = new SimplePriorityQueue<NodeRecord>();


	// Returns the NodeRectord structure in the list with the lowest costSoFar
	public NodeRecord SmallestElement()
	{
		if (queue.Count == 0)
		{
			return null;
		}

		return queue.Dequeue();
	}

	// Returns true only if the list contains a NodeRecord structure
	// whose node member is equal to the given parameter
	public bool Contains(Node node)
	{
		foreach (NodeRecord record in queue)
		{
			if (record.node.Equals(node))
			{
				return true;
			}
		}

		return false;
	}

	// Returns the NodeRecord structure from the list whose node member
	// is equal to the given parameter
	public NodeRecord Find(Node node)
	{
		foreach (NodeRecord record in queue)
		{
			if (record.node.Equals(node))
			{
				return record;
			}
		}

		return null;
	}

	// Adds the given NodeRecord structure to the list
	public void Add(NodeRecord nodeRecord)
	{
		queue.Enqueue(nodeRecord, nodeRecord.estimatedTotalCost);
	}

	// Removes the given NodeRecord structure from the list
	public void Remove(NodeRecord nodeRecord)
	{
		queue.Remove(nodeRecord);
	}

	// Overload the + operator to add a NodeRecord structure to the list
	public static PathFindingList operator +(PathFindingList list, NodeRecord nodeRecord)
	{
		list.Add(nodeRecord);
		return list;
	}

	// Overload the - operator to remove a NodeRecord structure from the list
	public static PathFindingList operator -(PathFindingList list, NodeRecord nodeRecord)
	{
		list.Remove(nodeRecord);
		return list;
	}

	// Returns the number of elements in the list
	public int Length()
	{
		return queue.Count;
	}
}