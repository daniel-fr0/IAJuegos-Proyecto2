using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class State : MonoBehaviour
{
	public string stateName;
	public List<Transition> transitions = new List<Transition>();
	public Kinematic kinematicData;

	void Awake()
	{
		kinematicData = GetComponent<Kinematic>();
		if (kinematicData == null)
		{
			Debug.LogWarning("Kinematic data not found in state " + stateName);
		}
	}

	void Start()
	{
		kinematicData = GetComponent<Kinematic>();
		if (kinematicData == null)
		{
			Debug.LogWarning("Kinematic data not found in state " + stateName);
		}
	}

	public Kinematic EnterState(Kinematic newKinematicData)
	{
		gameObject.SetActive(true);

		if (newKinematicData != null)
		{
			kinematicData.position = newKinematicData.position;
			kinematicData.velocity = newKinematicData.velocity;
			kinematicData.orientation = newKinematicData.orientation;
			kinematicData.rotation = newKinematicData.rotation;
		}

		return kinematicData;
	}

	public Kinematic ExitState()
	{
		gameObject.SetActive(false);
		
		return kinematicData;
	}
}