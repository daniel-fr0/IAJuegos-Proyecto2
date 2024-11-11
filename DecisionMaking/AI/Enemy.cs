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
	public RectTransform safeZoneRectangle;
	public int safeZoneLevel = 0;
	
	// World information
	public GameObject target;
	public GameObject item;
	private Node safeZoneNode;

	// Transition conditions
	public bool nearItem = false;
	public bool itemPickedUp = false;
	public bool nearTarget = false;
	public bool targetUnreachable = false;

	void DefineTransitions()
	{
		// Define transitions
		Transition patrolToChase = new Transition
		{
			transitionName = "PatrolToChase",
			targetState = chase,
			condition = () => NearTarget() && CanChaseTarget()
		};


		Transition chaseToPatrol = new Transition
		{
			transitionName = "ChaseToPatrol",
			targetState = patrol,
			condition = () => !CanChaseTarget()
		};

		Transition chaseToPickItem = new Transition
		{
			transitionName = "ChaseToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition gatherToChase = new Transition
		{
			transitionName = "PickItemToChase",
			targetState = chase,
			condition = () => PickedUpItem() && stateMachine.previousState == chase
		};

		Transition gatherToPatrol = new Transition
		{
			transitionName = "PickItemToPatrol",
			targetState = patrol,
			condition = () => PickedUpItem() && stateMachine.previousState == patrol
		};

		Transition patrolToPickItem = new Transition
		{
			transitionName = "PatrolToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		// Add transitions to states, gather has priority
		patrol.transitions.Add(patrolToPickItem);
		patrol.transitions.Add(patrolToChase);

		chase.transitions.Add(chaseToPickItem);
		chase.transitions.Add(chaseToPatrol);

		pickItem.transitions.Add(gatherToChase);
		pickItem.transitions.Add(gatherToPatrol);

		// Initialize state machine
		stateMachine.currentState = patrol;
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

		// Get safe zone node
		if (safeZoneRectangle != null && safeZoneLevel > 0)
		{
			safeZoneNode = new Node(safeZoneLevel);
			safeZoneNode.bounds = safeZoneRectangle.rect;
		}
		else
		{
			Debug.LogWarning("Safe zone not set for EnemyAI in " + gameObject.name);
		}
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
			itemPickedUp = true;
			nearItem = false;
			return true;
		}

		// If the item can be picked up
		if (Vector3.Distance(stateMachine.stateKinematicData.position, item.transform.position) < itemPickUpRadius)
		{
			// Pick up the item
			item.SetActive(false);
			itemPickedUp = true;
			nearItem = false;
			return true;
		}
		return false;
	}

	private bool NearTarget()
	{
		if (target.activeSelf == false)
		{
			return false;
		}
		return Vector3.Distance(stateMachine.stateKinematicData.position, target.transform.position) < detectionRadius;
	}

	private bool CanChaseTarget()
	{
		if (target.activeSelf == false)
		{
			return false;
		}

		return !safeZoneNode.Contains(new Node(target.transform.position));
	}

	private bool CanLookForItem()
	{
		if (item.activeSelf == false)
		{
			return false;
		}
		return Vector3.Distance(stateMachine.stateKinematicData.position, item.transform.position) < itemPickUpRadius;
	}
}