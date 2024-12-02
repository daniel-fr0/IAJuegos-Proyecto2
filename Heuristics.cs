using UnityEngine;


public abstract class Heuristic
{
	public Node goalNode;

	public Heuristic(Node goalNode)
	{
		this.goalNode = goalNode;
	}

	public float Estimate(Node fromNode)
	{
		return Estimate(fromNode, goalNode);
	}

	protected abstract float Estimate(Node fromNode, Node toNode);
}

public class EuclideanHeuristic : Heuristic
{
	public EuclideanHeuristic(Node goalNode) : base(goalNode)
	{
	}
	protected override float Estimate(Node fromNode, Node toNode)
	{
		Vector2 fromPosition = fromNode.GetPosition();
		Vector2 toPosition = toNode.GetPosition();

		return Vector2.Distance(fromPosition, toPosition);
	}
}

public class ManhattanHeuristic : Heuristic
{
	public ManhattanHeuristic(Node goalNode) : base(goalNode)
	{
	}
	protected override float Estimate(Node fromNode, Node toNode)
	{
		Vector2 fromPosition = fromNode.GetPosition();
		Vector2 toPosition = toNode.GetPosition();

		return Mathf.Abs(fromPosition.x - toPosition.x) + Mathf.Abs(fromPosition.y - toPosition.y);
	}
}

public class TacticalManhattanHeuristic : ManhattanHeuristic
{
	private float weight;
	public TacticalManhattanHeuristic(Node goalNode, float weight) : base(goalNode)
	{
		this.weight = weight;
	}
	protected override float Estimate(Node fromNode, Node toNode)
	{
		// Take into account the tacticalbenefit of the node
		return base.Estimate(fromNode, toNode) - weight*fromNode.tacticalBenefit;
	}
}
