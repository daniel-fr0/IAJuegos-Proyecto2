using System.Collections.Generic;
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