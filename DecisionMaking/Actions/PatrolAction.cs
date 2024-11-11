using UnityEngine;

public class PatrolAction: MonoBehaviour, Action
{
	// Main dynamic parameters
	public float maxSpeed = 10.0f;
	public float maxAcceleration = 100.0f;
	public float maxRotation = 5.0f;
	public float maxAngularAcceleration = 45.0f;

	// LookWhereYoureGoing parameters
	public float LWYGslowRadius = 1.0f;
	public float LWYGtimeToTarget = 0.01f;

	// PathFollowing parameters (followPathRabbit)
	public Path path;
	public float pathTargetOffset = 0.25f;

	// The action is implemented with seek, followPathRabbit and lookWhereYoureGoing behaviors
	public Seek seek;
	public FollowPathRabbit followPathRabbit;
	public LookWhereYoureGoing lwyg;

	// Base Kinematic parameters
	public Kinematic character;
	public Kinematic target;
	
	public void Start()
	{	
		// Initialize references
		seek = GetComponent<Seek>();
		followPathRabbit = GetComponent<FollowPathRabbit>();
		lwyg = GetComponent<LookWhereYoureGoing>();
		character = GetComponent<Kinematic>();

		// If any of the components are missing, add them
		if (seek == null)
		{
			seek = gameObject.AddComponent<Seek>();
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

		// Load followPathRabbit values
		followPathRabbit.path = path;
		followPathRabbit.targetOffset = pathTargetOffset;

		// Load lwyk values
		lwyg.maxAngularAcceleration = maxAngularAcceleration;
		lwyg.maxRotation = maxRotation;
		lwyg.slowRadius = LWYGslowRadius;
		lwyg.timeToTarget = LWYGtimeToTarget;

		// Set the target
		if (target == null) Debug.LogError("Target is null for PatrolAction in " + gameObject.name);
		else seek.target = target;		
	}

	public void Save()
	{
		path = followPathRabbit.path;
		target = seek.target;
	}

	public void OnStateEnter()
	{
		// Enable behaviors
		seek.enabled = true;
		followPathRabbit.enabled = true;
		lwyg.enabled = true;
	}

	public void OnStateExit()
	{
		// Disable behaviors
		seek.enabled = false;
		followPathRabbit.enabled = false;
		lwyg.enabled = false;
	}
}