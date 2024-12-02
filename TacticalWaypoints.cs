using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TacticalWaypoints : MonoBehaviour
{
    public GameObject positiveGroup;
    public GameObject negativeGroup;
    public Vector3[] positiveWaypoints;
    public Vector3[] negativeWaypoints;
}


[CustomEditor(typeof(TacticalWaypoints))]
public class TacticalWaypointsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TacticalWaypoints tacticalWaypoints = (TacticalWaypoints)target;

        if (GUILayout.Button("Calculate Waypoints"))
        {
            CalculateWaypoints(tacticalWaypoints);
            Debug.Log("Waypoints calculated!");
        }

        if (GUILayout.Button("Clear Waypoints"))
        {
            tacticalWaypoints.positiveWaypoints = new Vector3[0];
            tacticalWaypoints.negativeWaypoints = new Vector3[0];
            Debug.Log("Waypoints cleared!");
        }

        if (GUILayout.Button("Check Duplicates"))
        {
            CheckDuplicates(tacticalWaypoints);
        }
    }

    private void CalculateWaypoints(TacticalWaypoints tacticalWaypoints)
    {
        List<Vector3> positiveWaypointsList = new List<Vector3>();
        List<Vector3> negativeWaypointsList = new List<Vector3>();

        foreach (Transform child in tacticalWaypoints.positiveGroup.transform)
        {
            positiveWaypointsList.Add(child.gameObject.transform.position);
        }

        foreach (Transform child in tacticalWaypoints.negativeGroup.transform)
        {
            negativeWaypointsList.Add(child.gameObject.transform.position);
        }

        tacticalWaypoints.positiveWaypoints = positiveWaypointsList.ToArray();
        tacticalWaypoints.negativeWaypoints = negativeWaypointsList.ToArray();
    }

    private void CheckDuplicates(TacticalWaypoints tacticalWaypoints)
    {
        List<Vector3> positiveWaypointsList = new List<Vector3>();
        List<Vector3> negativeWaypointsList = new List<Vector3>();

        foreach (Vector3 waypoint in tacticalWaypoints.positiveWaypoints)
        {
            if (positiveWaypointsList.Contains(waypoint))
            {
                Debug.LogError("Duplicate waypoint found: " + waypoint);
            }
            else
            {
                positiveWaypointsList.Add(waypoint);
            }
        }

        foreach (Vector3 waypoint in tacticalWaypoints.negativeWaypoints)
        {
            if (negativeWaypointsList.Contains(waypoint))
            {
                Debug.LogError("Duplicate waypoint found: " + waypoint);
            }
            else
            {
                negativeWaypointsList.Add(waypoint);
            }
        }

        Debug.Log("Check complete!");
        Debug.Log("Positive waypoints: " + positiveWaypointsList.Count);
        Debug.Log("Negative waypoints: " + negativeWaypointsList.Count);
    }
}