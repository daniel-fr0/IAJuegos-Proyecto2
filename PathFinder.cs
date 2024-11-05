using System;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder instance;
    
    public Graph tileGraph;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private Connection[] pathFindAStar(Vector3 startPos, Vector3 endPos)
    {
        Node start = new Node(startPos);
        Node end = new Node(endPos);

        Heuristic heuristic = new ManhattanHeuristic(end);

        // Initialize the record for the start node
        NodeRecord startRecord = new NodeRecord(start, null, 0, heuristic.Estimate(start));

        // Initialize the open and closed lists
        PathFindingList open = new PathFindingList();
        open += startRecord;
        
        PathFindingList closed = new PathFindingList();

        // Iterate through processing each node
        // while (open.Count() > 0) {
        //     // To be continued...
        // }
    }
}
