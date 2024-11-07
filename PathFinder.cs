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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pfm = PathFinderManager.instance;
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
    }

    // Update is called once per frame
    void Update()
    {
		if (ReachedGoal()) return;

        if (path.numPoints != 0)
		{
			if (target != null && target.position != goalPosition)
			{
				goalPosition = target.position;
			}
			else return;
		}

		if (!TileClassifier.instance.IsWalkableTile(goalPosition)) return;
		if (!TileClassifier.instance.IsWalkableTile(transform.position)) return;
	
		connections = pfm.HierarchicalPathFindAStar(transform.position, goalPosition);

		if (connections == null || connections.Length == 0) return;

		// Create a new path
		Vector3[] newPoints = new Vector3[connections.Length + 1];

		// Add the points to the path
		for (int i = 0; i < newPoints.Length-1; i++)
		{
			Vector3 point = connections[i].fromNode.GetPosition();
			newPoints[i] = point;
		}

		if (newPoints.Length > 1)
			newPoints[newPoints.Length-1] = connections[connections.Length-1].toNode.GetPosition();

		// Overwrite the path points
		path.numPoints = newPoints.Length;
		path.points = newPoints;
    }

	private bool ReachedGoal()
	{
		bool completed = false;
		Vector3 distance = goalPosition - transform.position;
		Node goal = new Node(goalPosition);
		Node current = new Node(transform.position);

		if (!current.Equals(goal)) return false;

		if (distance.magnitude < targetRadius)
		{
			path.numPoints = 0;
			path.points = null;
			completed = true;
		}

		if (target != null && target.position != goalPosition)
		{
			goalPosition = target.position;
			completed = false;
		}

		return completed;
	}

	void LateUpdate()
	{
		if (ReachedGoal())
		{
			// Slow into a stop
			pathFollower.seeker.flee = true;
		}
		else
		{
			pathFollower.seeker.flee = false;
		}
	}
}
