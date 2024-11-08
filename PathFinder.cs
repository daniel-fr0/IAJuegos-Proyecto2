using UnityEngine;

public class PathFinder : MonoBehaviour
{
	public PathFollowing pathFollower;
	public Vector3 goalPosition;
	public Transform target;
	public float targetRadius = 0.5f;
	private GameObject pathHolder;
	private Path path = null;
	private Connection[] connections = null;
	private PathFinderManager pfm;
	private WorldRepresentation wrld;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pfm = PathFinderManager.instance;
		wrld = WorldRepresentation.instance;
		if (target != null)
		{
			goalPosition = target.position;
		}

		// Create a path holder
		pathHolder = new GameObject("Path Finder Holder");
		pathHolder.transform.parent = transform;
		pathHolder.AddComponent<Path>();
		path = pathHolder.GetComponent<Path>();
		path.looped = false;
		path.hideSprite = false;

		// Set the path to the pathFollower
		pathFollower.path = path;

		// Be able to use flee behavior to slow down
		pathFollower.seeker.fleeRadius = 0;
    }

    // Update is called once per frame
    void Update()
    {
		if (ReachedGoal())
		{
			path.points = null;
			return;
		}
	
		connections = pfm.HierarchicalPathFindAStar(transform.position, goalPosition);

		if (connections == null || connections.Length == 0) return;

		// Create a new path, it has 1 more point than the connections
		Vector3[] newPoints = new Vector3[connections.Length + 1];

		// Add the points to the path
		for (int i = 0; i < newPoints.Length-1; i++)
		{
			Vector3 point = connections[i].fromNode.GetPosition();
			newPoints[i] = point;
		}

		// Add the last point of the path
		newPoints[newPoints.Length-1] = connections[connections.Length-1].toNode.GetPosition();

		// Overwrite the path points
		path.points = newPoints;
    }

	private bool ReachedGoal()
	{
		if (target != null && target.position != goalPosition && wrld.IsWalkableTile(target.position))
		{
			goalPosition = target.position;
		}

		Vector3 distance = goalPosition - transform.position;
		Node goal = new Node(goalPosition);
		Node current = new Node(transform.position);

		if (distance.magnitude < targetRadius) return true;

		return current.Equals(goal);
	}

	void LateUpdate()
	{
		if (ReachedGoal())
		{
			// Slow into a stop
			// With the flee radius as 0, nothing will trigger the flee behavior
			pathFollower.seeker.flee = true;
		}
		else
		{
			pathFollower.seeker.flee = false;
		}
	}
}
