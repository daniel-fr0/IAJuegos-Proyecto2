using UnityEngine;

public class EvadeAction: MonoBehaviour, Action
{
	// Main dynamic parameters
	public float maxSpeed = 10.0f;
	public float maxAcceleration = 100.0f;
	public float maxRotation = 5.0f;
	public float maxAngularAcceleration = 45.0f;

	// Flee parameters
	public float fleeRadius = 1.0f;
	public float fleeTimeToStop = 0.01f;

	// LookWhereYoureGoing parameters
	public float LWYGslowRadius = 1.0f;
	public float LWYGtimeToTarget = 0.01f;

	// The action is implemented with seek(flee) and lookWhereYoureGoing behaviors
	public Seek seek;
	public LookWhereYoureGoing lwyg;

	// Base Kinematic parameters
	public Kinematic character;
	public Kinematic target;
	
	public void Start()
	{	
		// Initialize references
		seek = GetComponent<Seek>();
		lwyg = GetComponent<LookWhereYoureGoing>();
		character = GetComponent<Kinematic>();

		// If any of the components are missing, add them
		if (seek == null)
		{
			seek = gameObject.AddComponent<Seek>();
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
		seek.flee = true;
		seek.fleeRadius = fleeRadius;
		seek.timeToStop = fleeTimeToStop;

		// Load lwyk values
		lwyg.maxAngularAcceleration = maxAngularAcceleration;
		lwyg.maxRotation = maxRotation;
		lwyg.slowRadius = LWYGslowRadius;
		lwyg.timeToTarget = LWYGtimeToTarget;

		// Set the target
		if (target == null) Debug.LogError("Target is null for EvadeAction in " + gameObject.name);
		seek.target = target;
	}

	public void Save()
	{
		
	}

	public void OnStateEnter()
	{
		// Enable behaviors
		seek.enabled = true;
		lwyg.enabled = true;
	}

	public void OnStateExit()
	{
		// Disable behaviors
		seek.enabled = false;
		lwyg.enabled = false;
	}
}