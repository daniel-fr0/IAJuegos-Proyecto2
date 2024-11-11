using UnityEngine;
using System.Collections.Generic;

public class State
{
	public string stateName;
	public List<Action> stateActions = new List<Action>();
	public List<Transition> transitions = new List<Transition>();

	public void EnterState()
	{
		// Load any actions assigned to this state
		// and execute what needs to be done when entering the state
		foreach (Action action in stateActions)
		{
			action.Load();
			action.OnStateEnter();
			(action as MonoBehaviour).enabled = true;
		}
	}

	public void ExitState()
	{
		// Execute what needs to be done when exiting the state and
		// save the state of current actions
		foreach (Action action in stateActions)
		{
			action.Save();
			action.OnStateExit();
			(action as MonoBehaviour).enabled = false;
		}
	}
}