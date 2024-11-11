using UnityEngine;

public class ChaseAction: MonoBehaviour, Action
{
	// Main dynamic parameters
	public float maxSpeed = 10.0f;
	public float maxAcceleration = 100.0f;
	public float maxRotation = 5.0f;
	public float maxAngularAcceleration = 45.0f;

	// LookWhereYoureGoing parameters
	public float LWYGslowRadius = 1.0f;
	public float LWYGtimeToTarget = 0.01f;

	// Arrive parameters
	public float arriveTargetRadius = 0.5f;
	public float arriveSlowRadius = 1.0f;
	public float arriveTimeToTarget = 0.01f;

	// PathFinder parameters
	public float nodeTargetRadius = 0.5f;
	public float nodeSlowRadius = 1.0f;
	public float nodeTimeToTarget = 0.01f;

	// PathFollowing parameters (followPathRabbit)
	public Path path;
	public float pathTargetOffset = 0.25f;

	// The action is implemented with seek, arrive, pathfinder, followPathRabbit and lookWhereYoureGoing behaviors
	public Seek seek;
	public Arrive arrive;
	public PathFinder pathFinder;
	public FollowPathRabbit followPathRabbit;
	public LookWhereYoureGoing lwyg;

	// Base Kinematic parameters
	public Kinematic character;
	public Kinematic target;
	
	public void Start()
	{	
		// Initialize references
		seek = GetComponent<Seek>();
		arrive = GetComponent<Arrive>();
		pathFinder = GetComponent<PathFinder>();
		followPathRabbit = GetComponent<FollowPathRabbit>();
		lwyg = GetComponent<LookWhereYoureGoing>();
		character = GetComponent<Kinematic>();

		// If any of the components are missing, add them
		if (seek == null)
		{
			seek = gameObject.AddComponent<Seek>();
		}
		if (arrive == null)
		{
			arrive = gameObject.AddComponent<Arrive>();
		}
		if (pathFinder == null)
		{
			pathFinder = gameObject.AddComponent<PathFinder>();
		}
		if (followPathRabbit == null)
		{
			followPathRabbit = gameObject.AddComponent<FollowPathRabbit>();
		}
		if (lwyg == null)
		{
			lwyg = gameObject.AddComponent<LookWhereYoureGoing>();
		}
		if (character == null)
		{
			character = gameObject.AddComponent<Kinematic>();
		}
	}

	public void Load()
	{
		// Load seek values
		seek.target = target;
		seek.maxSpeed = maxSpeed;
		seek.maxAcceleration = maxAcceleration;

		// Load arrive values
		arrive.target = target;
		arrive.maxSpeed = maxSpeed;
		arrive.maxAcceleration = maxAcceleration;
		arrive.targetRadius = arriveTargetRadius;
		arrive.slowRadius = arriveSlowRadius;
		arrive.timeToTarget = arriveTimeToTarget;

		// Load lwyk values
		lwyg.maxAngularAcceleration = maxAngularAcceleration;
		lwyg.maxRotation = maxRotation;
		lwyg.slowRadius = LWYGslowRadius;
		lwyg.timeToTarget = LWYGtimeToTarget;

		// Set the target
		if (target == null) Debug.LogError("Target is null for ChaseAction in " + gameObject.name);
		pathFinder.target = target;
	}

	public void OnStateEnter()
	{
		
	}

	public void OnStateExit()
	{
		
	}

	public void Save()
	{
		
	}
}