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
	public Transform target;
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

	#region Input System
	private InputSystem_Actions controls;
	private void Awake()
	{
		controls = new InputSystem_Actions();
		controls.DebugUI.ToggleMap.performed += ctx => path.debugInfo = !path.debugInfo;
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

		// Do not recalculate if there is a path already leading to the goal
		if (path.points != null && path.points.Length > 0)
		{
			Node goalNode = new Node(goalPosition);
			Vector3 endPosition = path.points[path.points.Length-1];

			if (goalNode.GetPosition() == endPosition) return;
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
}
