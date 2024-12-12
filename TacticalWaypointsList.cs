using UnityEngine;
using System;
using System.Collections.Generic;

public class TacticalWaypointsList : MonoBehaviour
{
	public float tacticalBenefit;
	public Waypoint[] waypoints;
	public bool showGizmos = true;

	#if UNITY_EDITOR
		void OnValidate()
		{
			UpdateWaypoints();
		}
	#endif

	public void UpdateWaypoints()
	{
		List<Waypoint> tacticalWaypoints = new List<Waypoint>();
		foreach (Transform child in transform)
		{
			Waypoint waypoint = new Waypoint
			{
				tacticalBenefit = tacticalBenefit,
				position = child.position
			};
			tacticalWaypoints.Add(waypoint);
		}
		waypoints = tacticalWaypoints.ToArray();
	}

	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (!showGizmos)
		{
			return;
		}

		foreach (Waypoint child in waypoints)
		{
			Gizmos.color = child.tacticalBenefit > 0 ? new Color(0, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
			Gizmos.DrawSphere(child.position, MathF.Abs(tacticalBenefit) * 0.5f);
		}
	}
}

[Serializable]
public class Waypoint
{
	public float tacticalBenefit;
	public Vector3 position;
}