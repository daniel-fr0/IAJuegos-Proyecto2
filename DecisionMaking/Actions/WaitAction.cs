using UnityEngine;

public class WaitAction: MonoBehaviour, Action
{
	// The speed at which the character moves around and turns
	public float maxSpeed = 10.0f;
	public float maxAcceleration = 100.0f;
	public float maxRotation = 5.0f;
	public float maxAngularAcceleration = 45.0f;

	// LookWhereYoureGoing parameters
	public float LWYGslowRadius = 1.0f;
	public float LWYGtimeToTarget = 0.01f;

	// The action is implemented with a seek behavior
	public Seek seek;

	// To work, seek needs the following
	public Kinematic character;
	public LookWhereYoureGoing lwyg;
	public Kinematic target;
	
	public void Start()
	{	
		// Initialize references
		seek = GetComponent<Seek>();
		character = GetComponent<Kinematic>();
		lwyg = GetComponent<LookWhereYoureGoing>();

		// If any of the components are missing, add them
		if (seek == null)
		{
			seek = gameObject.AddComponent<Seek>();
		}
		if (character == null)
		{
			character = gameObject.AddComponent<Kinematic>();
		}
		if (lwyg == null)
		{
			lwyg = gameObject.AddComponent<LookWhereYoureGoing>();
		}

		// add target
		GameObject targetObject = new GameObject("WaitTarget");
		target = targetObject.AddComponent<Kinematic>();
	}

	public void Load()
	{
		// Load seek values
		seek.target = target;
		seek.maxSpeed = maxSpeed;
		seek.maxAcceleration = maxAcceleration;
		seek.flee = false;

		// Load lwyk values
		lwyg.maxAngularAcceleration = maxAngularAcceleration;
		lwyg.maxRotation = maxRotation;
		lwyg.slowRadius = LWYGslowRadius;
		lwyg.timeToTarget = LWYGtimeToTarget;

		// Set the target
		Vector3 nodePosition = new Node(target.position).GetPosition();
		target.position = nodePosition;
		seek.target = target;
	}

	public void Save()
	{

	}

	public void OnStateEnter()
	{
		// Enable all behaviors
		seek.enabled = true;
		lwyg.enabled = true;
	}

	public void OnStateExit()
	{
		// Disable all behaviors
		seek.enabled = false;
		lwyg.enabled = false;
	}
}