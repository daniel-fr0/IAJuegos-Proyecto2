using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class State
{
	public string stateName;
	public List<MonoBehaviour> stateBehaviours = new List<MonoBehaviour>();
	public List<Transition> transitions = new List<Transition>();

	private Dictionary<MonoBehaviour, object> savedProperties = new Dictionary<MonoBehaviour, object>();

	public void EnterState()
	{
		// Load any actions assigned to this state
		// and execute what needs to be done when entering the state
		foreach (MonoBehaviour behaviour in stateBehaviours)
		{
			if (behaviour != null)
			{
				LoadProperties(behaviour);
				behaviour.enabled = true;
			}
		}
	}

	public void ExitState()
	{
		// Execute what needs to be done when exiting the state and
		// save the state of current actions
		foreach (MonoBehaviour behaviour in stateBehaviours)
		{
			if (behaviour != null)
			{
				behaviour.enabled = false;
				SaveProperties(behaviour);
			}
		}
	}

	public void SaveProperties(MonoBehaviour behaviour)
	{
		var type = behaviour.GetType();
		var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		var propertyValues = new Dictionary<string, object>();
		foreach (var field in fields)
		{
			propertyValues[field.Name] = field.GetValue(behaviour);
		}
		savedProperties[behaviour] = propertyValues;
	}

	public void LoadProperties(MonoBehaviour behaviour)
	{
		if (savedProperties.ContainsKey(behaviour))
		{
			var type = behaviour.GetType();
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			var propertyValues = savedProperties[behaviour] as Dictionary<string, object>;
			foreach (var field in fields)
			{
				if (propertyValues.ContainsKey(field.Name))
				{
					field.SetValue(behaviour, propertyValues[field.Name]);
				}
			}
		}
	}
}