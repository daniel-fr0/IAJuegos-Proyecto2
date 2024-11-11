using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public StateMachine stateMachine;
	public GameObject worldStatePrefab;
	private WorldState WS;

	// State
	public State chase;
	public State patrol;
	public State pickItem;

	// Transition parameters
	public float detectionRadius = 3.0f;
	public float itemPickUpRadius = 1.0f;
	
	// World information
	private Kinematic character;
	private Kinematic target;
	private Kinematic item;

	void Awake() {
		// Define transitions
		Transition patrolToChase = new Transition
		{
			transitionName = "PatrolToChase",
			targetState = chase,
			condition = () => target != null
		};


		Transition chaseToPatrol = new Transition
		{
			transitionName = "ChaseToPatrol",
			targetState = patrol,
			condition = () => target == null
		};

		Transition chaseToPickItem = new Transition
		{
			transitionName = "ChaseToPickItem",
			targetState = pickItem,
			condition = () => item != null
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
			condition = () => item != null
		};

		// Add transitions to states, gather has priority
		patrol.transitions.Add(patrolToPickItem);
		patrol.transitions.Add(patrolToChase);

		chase.transitions.Add(chaseToPickItem);
		chase.transitions.Add(chaseToPatrol);

		pickItem.transitions.Add(gatherToChase);
		pickItem.transitions.Add(gatherToPatrol);

		// Initialize state machine
		stateMachine.initialState = patrol;
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
		character = GetComponent<Kinematic>();
	}

	void Update()
	{
		if (item == null)
		{
			// Look for items in the detection radius
			foreach (Kinematic i in WS.items)
			{
				if (Vector3.Distance(character.position, i.position) < detectionRadius)
				{
					item = i;
					break;
				}
			}
		}
		else
		{
			// If the item was picked up by someone else
			if (!WS.items.Contains(item))
			{
				item = null;
			}
		}

		if (target == null)
		{
			// Look for targets in the detection radius
			foreach (Kinematic f in WS.friendly)
			{
				if (Vector3.Distance(character.position, f.position) < detectionRadius)
				{
					target = f;
					break;
				}
			}
		}
		else
		{
			// If the target reached a safe zone
			foreach (Node n in WS.safeZones)
			{
				if (n.Contains(new Node(target.position)))
				{
					target = null;
					break;
				}
			}
		}
	}

	public bool PickedUpItem()
	{
		// If the item was picked up by someone else
		if (item == null) return false;

		// If the item can be picked up
		if (Vector3.Distance(character.position, item.position) < itemPickUpRadius)
		{
			// Pick up the item
			WS.items.Remove(item);
			Destroy(item.gameObject);
			item = null;
			return true;
		}
		return false;
	}
}