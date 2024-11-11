using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public StateMachine stateMachine;
	private Kinematic character;
	public Kinematic target;
	public float detectionRadius = 10.0f;
	public float itemPickUpRadius = 1.0f;
	public float itemPickUpTime = 0.5f;
	public Kinematic item;
	public GameObject worldStatePrefab;

	public Dictionary<string, Action> actions = new Dictionary<string, Action>();

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
		}

		// Initialize references
		character = GetComponent<Kinematic>();


		if (target == null) Debug.LogError("Target not set for EnemyAI in " + gameObject.name);

		// Define states and transitions
		State patrolState = new State {stateName = "Patrol"};
		patrolState.stateActions.Add(actions["Patrol"]);

		State chaseState = new State {stateName = "Chase"};
		chaseState.stateActions.Add(actions["Chase"]);

		State gatherState = new State {stateName = "Gather"};
		gatherState.stateActions.Add(actions["Gather"]);


		Transition patrolToChase = new Transition
		{
			transitionName = "PatrolToChase",
			targetState = chaseState,
			condition = () => Vector3.Distance(character.position, target.position) < detectionRadius
		};


		Transition chaseToPatrol = new Transition
		{
			transitionName = "ChaseToPatrol",
			targetState = patrolState,
			condition = () => Vector3.Distance(character.position, target.position) > detectionRadius
		};

		Transition chaseToGather = new Transition
		{
			transitionName = "ChaseToGather",
			targetState = gatherState,
			condition = () => item != null
		};

		Transition gatherToChase = new Transition
		{
			transitionName = "GatherToChase",
			targetState = chaseState,
			condition = () => PickUpItem()
		};

		Transition gatherToPatrol = new Transition
		{
			transitionName = "GatherToPatrol",
			targetState = patrolState,
			condition = () => PickUpItem()
		};

		Transition patrolToGather = new Transition
		{
			transitionName = "PatrolToGather",
			targetState = gatherState,
			condition = () => item != null
		};

		// Add transitions to states, gather has priority
		patrolState.transitions.Add(patrolToGather);
		patrolState.transitions.Add(patrolToChase);

		chaseState.transitions.Add(chaseToGather);
		chaseState.transitions.Add(chaseToPatrol);

		gatherState.transitions.Add(gatherToChase);
		gatherState.transitions.Add(gatherToPatrol);
	}

	void Update()
	{
		if (item != null) return;

		// Look for items in the detection radius
		foreach (Kinematic item in WorldState.instance.items)
		{
			if (Vector3.Distance(character.position, item.position) < detectionRadius)
			{
				this.item = item.GetComponent<Kinematic>();
				break;
			}
		}
	}

	public bool PickUpItem()
	{
		// If the item was picked up by someone else
		if (item == null) return false;

		if (Vector3.Distance(character.position, item.position) < itemPickUpRadius)
		{
			// Pick up the item
			Destroy(item.gameObject);
			return true;
		}
		return false;
	}
}