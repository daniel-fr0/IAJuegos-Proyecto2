using UnityEngine;

public class StalkerAI : MonoBehaviour
{
	public StateMachineAI stateMachine;

	// States
	public State followPlayer;
	public State waitStandingStill;
	public State fleeFromPlayer;
	public State pickItem;

	// World information
	public GameObject player;
	public GameObject item;

	// Transition parameters
	public float followRadius = 4.0f;
	public float fleeRadius = 3.0f;
	public float itemDetectionRadius = 3.0f;
	public float itemPickUpRadius = 1.0f;
	public bool debugInfo = false;

	void DefineTransitions()
	{
		// Define transitions
		Transition waitToFollow = new Transition
		{
			transitionName = "WaitToFollow",
			targetState = followPlayer,
			condition = () => TooFarFromPlayer()
		};

		Transition followToWait = new Transition
		{
			transitionName = "FollowToWait",
			targetState = waitStandingStill,
			condition = () => JustRightFromPlayer()
		};

		Transition waitToFlee = new Transition
		{
			transitionName = "WaitToFlee",
			targetState = fleeFromPlayer,
			condition = () => TooNearToPlayer()
		};

		Transition fleeToWait = new Transition
		{
			transitionName = "FleeToWait",
			targetState = waitStandingStill,
			condition = () => JustRightFromPlayer()
		};

		Transition followToPickItem = new Transition
		{
			transitionName = "FollowToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition pickItemToFollow = new Transition
		{
			transitionName = "PickItemToFollow",
			targetState = followPlayer,
			condition = () => PickedUpItem() && stateMachine.previousState == followPlayer
		};

		Transition waitToPickItem = new Transition
		{
			transitionName = "WaitToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition pickItemToWait = new Transition
		{
			transitionName = "PickItemToWait",
			targetState = waitStandingStill,
			condition = () => PickedUpItem() && stateMachine.previousState == waitStandingStill
		};

		Transition fleeToPickItem = new Transition
		{
			transitionName = "FleeToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition pickItemToFlee = new Transition
		{
			transitionName = "PickItemToFlee",
			targetState = fleeFromPlayer,
			condition = () => PickedUpItem() && stateMachine.previousState == fleeFromPlayer
		};

		// Add transitions to states, pickItem has priority
		waitStandingStill.transitions.Add(waitToPickItem);
		waitStandingStill.transitions.Add(waitToFollow);
		waitStandingStill.transitions.Add(waitToFlee);

		followPlayer.transitions.Add(followToPickItem);
		followPlayer.transitions.Add(followToWait);

		fleeFromPlayer.transitions.Add(fleeToPickItem);
		fleeFromPlayer.transitions.Add(fleeToWait);

		pickItem.transitions.Add(pickItemToFollow);
		pickItem.transitions.Add(pickItemToWait);
		pickItem.transitions.Add(pickItemToFlee);

		// Set initial state
		stateMachine.currentState = waitStandingStill;
	}

	void Start()
	{
		// Check if the states are set
		if (followPlayer == null || waitStandingStill == null || fleeFromPlayer == null || pickItem == null)
		{
			Debug.LogError("States not set for StalkerAI in " + gameObject.name);
			return;
		}

		// Check if the state machine is set
		if (stateMachine == null)
		{
			Debug.LogError("State machine not set for StalkerAI in " + gameObject.name);
			return;
		}

		// Check if the player is set
		if (player == null)
		{
			Debug.LogError("Player not set for StalkerAI in " + gameObject.name);
			return;
		}

		// Check if the item is set
		if (item == null)
		{
			Debug.LogError("Item not set for StalkerAI in " + gameObject.name);
			return;
		}

		// Define transitions
		DefineTransitions();
	}

	void Update()
	{
		if (debugInfo)
		{
			DebugVisuals.DrawRadius(stateMachine.stateKinematicData.position, followRadius, Color.green);
			DebugVisuals.DrawRadius(stateMachine.stateKinematicData.position, fleeRadius, Color.yellow);
			DebugVisuals.DrawRadius(stateMachine.stateKinematicData.position, itemDetectionRadius, Color.red);
		}
	}

	private bool TooFarFromPlayer()
	{
		return Vector3.Distance(player.transform.position, transform.position) > followRadius;
	}

	private bool TooNearToPlayer()
	{
		return Vector3.Distance(player.transform.position, transform.position) < fleeRadius;
	}

	private bool JustRightFromPlayer()
	{
		return Vector3.Distance(player.transform.position, transform.position) <= followRadius && Vector3.Distance(player.transform.position, transform.position) >= fleeRadius;
	}

	private bool CanLookForItem()
	{
		if (item.activeSelf == false)
		{
			return false;
		}
		return Vector3.Distance(item.transform.position, transform.position) < itemDetectionRadius;
	}

	private bool PickedUpItem()
	{
		// If the item was picked up by someone else
		if (item.activeSelf == false)
		{
			return true;
		}

		// If the item can be picked up
		if (Vector3.Distance(transform.position, item.transform.position) < itemPickUpRadius)
		{
			// Pick up the item
			item.SetActive(false);
			return true;
		}
		return false;
	}
}