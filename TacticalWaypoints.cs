using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TacticalWaypoints : MonoBehaviour
{
    public Waypoint[] waypoints;

    #if UNITY_EDITOR
        void OnValidate()
        {
            UpdateWaypoints();
        }
    #endif

    public void UpdateWaypoints()
    {
        TacticalWaypointsList[] tacticalWaypointsLists = GetComponentsInChildren<TacticalWaypointsList>();

        List<Waypoint> wpList = new List<Waypoint>();

        foreach (TacticalWaypointsList tacticalWaypointsList in tacticalWaypointsLists)
        {
            tacticalWaypointsList.UpdateWaypoints();
            wpList.AddRange(tacticalWaypointsList.waypoints);
        }

        waypoints = wpList.ToArray();
    }
}


[CustomEditor(typeof(TacticalWaypoints))]
public class TacticalWaypointsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Waypoints"))
        {
            TacticalWaypoints tacticalWaypoints = (TacticalWaypoints)target;
            tacticalWaypoints.UpdateWaypoints();
            Debug.Log("Waypoints updated!");
        }

        if (GUILayout.Button("Check Duplicates"))
        {
            TacticalWaypoints tacticalWaypoints = (TacticalWaypoints)target;
            Waypoint[] waypoints = tacticalWaypoints.waypoints;

            HashSet<Vector3> positions = new HashSet<Vector3>();

            foreach (Waypoint waypoint in waypoints)
            {
                if (!positions.Add(waypoint.position))
                {
                    Debug.LogError("Duplicate waypoint at " + waypoint.position);
                }
            }

            Debug.Log("Duplicates checked!");
        }
    }
}