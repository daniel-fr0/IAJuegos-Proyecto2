using UnityEngine;

public class PathFinder : MonoBehaviour
{
	public PathFollowing pathFollower;
	private Vector3 _goalPosition;
	public Vector3 goalPosition
	{
		get { return _goalPosition; }
		set {
			if (wrld.IsWalkableTile(value))
			{
				// Set the goal position as the position of the level 0 node
				_goalPosition = new Node(value).GetPosition();
			}
		}
	}
	public Kinematic target;
	private GameObject pathHolder;
	private Path path = null;
	private Connection[] connections = null;
	private PathFinderManager pfm;
	private WorldRepresentation wrld;

	// To manage slow down and arrive
	private Arrive arrive;
	private Seek seek;
	public float maxAcceleration = 100.0f;
	public float maxSpeed = 10.0f;
	public float nodeTargetRadius = 0.5f;
	public float nodeSlowRadius = 1.0f;
	public float nodeTimeToTarget = 0.1f;

	// To toggle between normal and hierarchical path finding
	public bool precisePathFinding = false;
	public bool tacticalPathfinding = false;
	public float tacticalWeight = 1;
	public bool debugInfo = false;

	#region Input System
	private InputSystem_Actions controls;
	private void Awake()
	{
		controls = new InputSystem_Actions();
		controls.DebugUI.ToggleMap.performed += ctx => debugInfo = !debugInfo;
	}
	private void OnEnable()
	{
		controls.Enable();
	}
	private void OnDisable()
	{
		controls.Disable();
	}
	#endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pfm = PathFinderManager.instance;
		if (tacticalPathfinding)
		{
			pfm.tacticalPathfinding = true;
			pfm.tacticalWeight = tacticalWeight;
		}
		wrld = WorldRepresentation.instance;
		if (target != null)
		{
			goalPosition = target.position;
		}

		// Create a path holder
		pathHolder = new GameObject(gameObject.name + "Path for PathFinder");
		pathHolder.transform.parent = transform;
		pathHolder.AddComponent<Path>();
		path = pathHolder.GetComponent<Path>();
		path.looped = false;
		path.hideSprite = false;

		// Set the path to the pathFollower
		pathFollower.path = path;

		// Set seek/arrive behavior properties
		seek = transform.GetComponent<Seek>();
		arrive = transform.GetComponent<Arrive>();

		if (seek == null || arrive == null)
		{
			Debug.LogWarning("Seek or Arrive behavior missing in " + gameObject.name);
			return;
		}

		
		seek.enabled = true;
		seek.maxAcceleration = maxAcceleration;
		seek.maxSpeed = maxSpeed;

		arrive.enabled = false;
		arrive.maxAcceleration = maxAcceleration;
		arrive.maxSpeed = maxSpeed;
		arrive.targetRadius = nodeTargetRadius;
		arrive.slowRadius = nodeSlowRadius;
		arrive.timeToTarget = nodeTimeToTarget;

		// Create arrive point
		GameObject arrivePointHolder = new GameObject("Arrive Point");
		arrivePointHolder.transform.parent = transform;
		Kinematic arrivePoint = arrivePointHolder.AddComponent<Kinematic>();
		arrivePoint.freezePosition = true;
		arrive.target = arrivePoint;
    }

    // Update is called once per frame
    void Update()
    {
		if (ReachedGoal())
		{
			path.points = null;
			return;
		}

		if (!wrld.IsWalkableTile(transform.position))
		{
			Debug.LogWarning("Character is not on a walkable tile!");
			return;
		}

		if (precisePathFinding)
		{
			connections = pfm.PathFindAStar(transform.position, goalPosition);
		}
		else
		{
			connections = pfm.HierarchicalPathFindAStar(transform.position, goalPosition);
		}

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

		// Make the goal the arrive target, also add the offset because it is a child of the path finder
		arrive.target.position = goalPosition;

		DrawPaths(connections);
    }

	private bool ReachedGoal()
	{
		if (target != null && target.position != goalPosition)
		{
			goalPosition = target.position;
		}

		Vector3 distance = goalPosition - transform.position;
		Node goal = new Node(goalPosition);
		Node current = new Node(transform.position);

		return Arrive(distance.magnitude < nodeSlowRadius || current.Equals(goal));
	}

	private bool Arrive(bool nearTarget)
	{
		// If the character is near the target, stop moving using arrive behavior
		Seeker newSeeker = nearTarget ? arrive : seek;
		
		arrive.enabled = nearTarget;
		seek.enabled = !nearTarget;
		pathFollower.seeker = newSeeker;
		
		return nearTarget;
	}

	void DrawPaths(Connection[] connections)
	{
		path.debugInfo = debugInfo;
		pfm.tacticalPathfinding = tacticalPathfinding;
		pfm.tacticalWeight = tacticalWeight;		

		if (!debugInfo) return;
		
		if (tacticalPathfinding)
		{
			Connection[] OGconnections;
			pfm.tacticalPathfinding = false;

			// calculate original path and display it
			if (precisePathFinding)
			{
				OGconnections = pfm.PathFindAStar(transform.position, goalPosition);
			}
			else
			{
				OGconnections = pfm.HierarchicalPathFindAStar(transform.position, goalPosition);
			}

			if (OGconnections == null || OGconnections.Length == 0) return;

			// Draw the original path
			foreach (Connection connection in OGconnections)
			{
				Debug.DrawLine(connection.fromNode.GetPosition(), connection.toNode.GetPosition(), Color.red);
			}

			pfm.tacticalPathfinding = true;
		}
	}
}
