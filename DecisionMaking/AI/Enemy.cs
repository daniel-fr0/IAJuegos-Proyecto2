using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public StateMachineAI stateMachine;

	// State
	public State patrol;
	public State chase;
	public State pickItem;
	public State chasePatrol;

	// World information
	public GameObject target;
	public GameObject item;
	public RectTransform safeZoneRectangle;
	public int safeZoneLevel = 1;
	private Node safeZoneNode;

	// Transition parameters
	public float detectionRadius = 3.0f;
	public float itemPickUpRadius = 1.0f;
	public float patrolArriveRadius = 0.25f;
	public bool debugInfo = false;

	void DefineTransitions()
	{
		// Define transitions
		Transition patrolToChase = new Transition
		{
			transitionName = "PatrolToChase",
			targetState = chase,
			condition = () => NearTarget() && CanChaseTarget()
		};

		Transition chaseToPickItem = new Transition
		{
			transitionName = "ChaseToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition pickItemToChase = new Transition
		{
			transitionName = "PickItemToChase",
			targetState = chase,
			condition = () => PickedUpItem() && stateMachine.previousState == chase
		};

		Transition patrolToPickItem = new Transition
		{
			transitionName = "PatrolToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};

		Transition pickItemToPatrol = new Transition
		{
			transitionName = "PickItemToPatrol",
			targetState = chasePatrol,
			condition = () => PickedUpItem() && (stateMachine.previousState == patrol || stateMachine.previousState == chasePatrol) && ReturnToPatrol()
		};

		Transition chaseToPatrol = new Transition
		{
			transitionName = "ChaseToPatrol",
			targetState = chasePatrol,
			condition = () => !CanChaseTarget() && ReturnToPatrol()
		};

		Transition chasePatrolToPatrol = new Transition
		{
			transitionName = "ChasePatrolToPatrol",
			targetState = patrol,
			condition = () => ArrivedToPatrol()
		};

		Transition chasePatrolToChase = new Transition
		{
			transitionName = "ChasePatrolToChase",
			targetState = chase,
			condition = () => NearTarget() && CanChaseTarget()
		};

		Transition chasePatrolToPickItem = new Transition
		{
			transitionName = "ChasePatrolToPickItem",
			targetState = pickItem,
			condition = () => CanLookForItem()
		};
		

		// Add transitions to states, pickItem has priority
		patrol.transitions.Add(patrolToPickItem);
		patrol.transitions.Add(patrolToChase);

		chase.transitions.Add(chaseToPickItem);
		chase.transitions.Add(chaseToPatrol); // First goes to chasePatrol to return to patrol

		pickItem.transitions.Add(pickItemToChase);
		pickItem.transitions.Add(pickItemToPatrol); // First goes to chasePatrol to return to patrol

		chasePatrol.transitions.Add(chasePatrolToPickItem);
		chasePatrol.transitions.Add(chasePatrolToChase);
		chasePatrol.transitions.Add(chasePatrolToPatrol);

		// Initialize state machine
		stateMachine.currentState = patrol;
	}

	void Start()
	{
		// Check if the states are set
		if (chase == null || patrol == null || pickItem == null)
		{
			Debug.LogError("States not set for EnemyAI in " + gameObject.name);
			return;
		}

		// Initialize references
		DefineTransitions();

		// Get safe zone node
		if (safeZoneRectangle != null)
		{
			safeZoneNode = new Node(safeZoneLevel);
			safeZoneNode.bounds = safeZoneRectangle.rect;
			safeZoneNode.center = safeZoneRectangle.position;
		}
		else
		{
			Debug.LogWarning("Safe zone not set for EnemyAI in " + gameObject.name);
		}

		// Start at current position
		stateMachine.currentState.transform.position = transform.position;

		// Hide sprite
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = false;
		}
	}
	
	void Update()
	{
		if (debugInfo)
		{
			DebugVisuals.DrawRadius(stateMachine.stateKinematicData.position, detectionRadius, Color.yellow);
		}
	}
	private bool PickedUpItem()
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
		return Vector3.Distance(stateMachine.stateKinematicData.position, item.transform.position) < detectionRadius;
	}

	private bool ReturnToPatrol()
	{
		chasePatrol.GetComponent<PathFinder>().target = patrol.kinematicData;
		return true;
	}

	private bool ArrivedToPatrol()
	{
		return Vector3.Distance(stateMachine.stateKinematicData.position, patrol.kinematicData.position) < patrolArriveRadius;
	}
}