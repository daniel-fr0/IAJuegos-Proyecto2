using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class State : MonoBehaviour
{
	public string stateName;
	public GameObject stateObject;
	public List<Transition> transitions = new List<Transition>();
	private Kinematic kinematicData;

	void Start()
	{
		kinematicData = GetComponent<Kinematic>();
	}

	public Kinematic EnterState(Kinematic newKinematicData)
	{
		stateObject.SetActive(true);

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
		stateObject.SetActive(false);
		
		return kinematicData;
	}
}