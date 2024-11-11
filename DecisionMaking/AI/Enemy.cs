using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public StateMachine stateMachine;
	public GameObject worldStatePrefab;
	private WorldState WS;

	// State
	public State patrol;
	public State chase;
	public State pickItem;

	// Transition parameters
	public float detectionRadius = 5.0f;
	public float itemPickUpRadius = 1.0f;
	
	// World information
	public GameObject target;
	public GameObject item;

	// Transition conditions
	private bool nearItem = false;
	private bool itemPickedUp = false;
	private bool nearTarget = false;
	private bool targetUnreachable = false;

	void DefineTransitions()
	{
		// Define transitions
		Transition patrolToChase = new Transition
		{
			transitionName = "PatrolToChase",
			targetState = chase,
			condition = () => nearTarget && !targetUnreachable
		};


		Transition chaseToPatrol = new Transition
		{
			transitionName = "ChaseToPatrol",
			targetState = patrol,
			condition = () => targetUnreachable
		};

		Transition chaseToPickItem = new Transition
		{
			transitionName = "ChaseToPickItem",
			targetState = pickItem,
			condition = () => nearItem && !itemPickedUp
		};

		Transition gatherToChase = new Transition
		{
			transitionName = "PickItemToChase",
			targetState = chase,
			condition = () => PickedUpItem()
		};

		Transition gatherToPatrol = new Transition
		{
			transitionName = "PickItemToPatrol",
			targetState = patrol,
			condition = () => PickedUpItem()
		};

		Transition patrolToPickItem = new Transition
		{
			transitionName = "PatrolToPickItem",
			targetState = pickItem,
			condition = () => nearItem
		};

		// Add transitions to states, gather has priority
		patrol.transitions.Add(patrolToPickItem);
		patrol.transitions.Add(patrolToChase);

		chase.transitions.Add(chaseToPickItem);
		chase.transitions.Add(chaseToPatrol);

		pickItem.transitions.Add(gatherToChase);
		pickItem.transitions.Add(gatherToPatrol);

		// Initialize state machine
		stateMachine.state = patrol;
	}

	void Start()
	{
		// Initialize world state
		if (WorldState.instance == null)
		{
			if (worldStatePrefab == null)
			{
				Debug.LogError("WorldState prefab not set for EnemyAI in " + gameObject.name);
				return;
			}
			Instantiate(worldStatePrefab);
			WS = WorldState.instance;
		}
		else
		{
			WS = WorldState.instance;
		}

		// Check if the states are set
		if (chase == null || patrol == null || pickItem == null)
		{
			Debug.LogError("States not set for EnemyAI in " + gameObject.name);
			return;
		}

		// Initialize references
		DefineTransitions();
	}

	void Update()
	{
		// Update the conditions
		if (item.activeSelf == false)
		{
			itemPickedUp = true;
		}
		else
		{
			nearItem = Vector3.Distance(stateMachine.stateKinematicData.position, item.transform.position) < detectionRadius;
			itemPickedUp = false;
		}

		foreach (Node safeZone in WS.safeZones)
		{
			if (safeZone.Contains(new Node(target.transform.position)))
			{
				targetUnreachable = true;
				return;
			}
		}

		targetUnreachable = false;

		nearTarget = Vector3.Distance(stateMachine.stateKinematicData.position, target.transform.position) < detectionRadius;
	}

	public bool PickedUpItem()
	{
		// If the item was picked up by someone else
		if (item.activeSelf == false)
		{
			return true;
		}

		// If the item can be picked up
		if (Vector3.Distance(stateMachine.stateKinematicData.position, item.transform.position) < itemPickUpRadius)
		{
			// Pick up the item
			item.SetActive(false);
			return true;
		}
		return false;
	}
}