using UnityEngine;
using System;

[Serializable]
public class Transition
{
	public string transitionName;
	public State targetState;
	public Func<bool> condition;

	public bool IsTriggered()
	{
		// If there's no condition, the transition can never be triggered
		if (condition == null)
		{
			Debug.LogError("Transition " + transitionName + " has no condition!");
			return false;
		}
		return condition();
	}
}