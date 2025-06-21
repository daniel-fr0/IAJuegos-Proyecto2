using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using System.Collections.Generic;

public class WorldRepresentation : MonoBehaviour
{
    public Tilemap tilemap;
    public TacticalWaypoints tacticalWaypoints;
    private PathFinderManager pfm;

    // Walkable tile ids
	public int[] walkableTiles = {0,1,8,9,11};

    #region Input System + Singleton Initialization
    private InputSystem_Actions controls;
    public static WorldRepresentation instance;

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Toggle debug info
        controls.DebugUI.ToggleMap.performed += ctx => debugInfo = !debugInfo;

        // Increase/decrease the number of levels shown
        controls.DebugUI.DecreaseMinLevel.performed += ctx => minGizmosLevels = Mathf.Clamp(minGizmosLevels-1, 0, maxGizmosLevels);
        controls.DebugUI.IncreaseMinLevel.performed += ctx => minGizmosLevels = Mathf.Clamp(minGizmosLevels+1, 0, maxGizmosLevels);

        controls.DebugUI.DecreaseMaxLevel.performed += ctx => maxGizmosLevels = Mathf.Clamp(maxGizmosLevels-1, minGizmosLevels, pfm.hierarchicalGraph.Height()-1);
        controls.DebugUI.IncreaseMaxLevel.performed += ctx => maxGizmosLevels = Mathf.Clamp(maxGizmosLevels+1, minGizmosLevels, pfm.hierarchicalGraph.Height()-1);

        // Toggle between bounding boxes/connection lines
        controls.DebugUI.ToggleGraphMode.performed += ctx => 
        {
            drawBoundingBoxes = !drawBoundingBoxes;
            drawConnections = !drawBoundingBoxes;
        };

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
    #endregion

    #region Gizmos + Debug
    // Debug variables
    public bool debugInfo = false;
    public int minGizmosLevels = 0;
    public int maxGizmosLevels = 0;
    public bool drawBoundingBoxes = true;
    public bool drawConnections = true;
    public bool drawSpheres = true;
    public Color[] colors = { Color.green, Color.red, Color.cyan, Color.magenta, Color.yellow, Color.blue, Color.white, Color.black };
    public WorldLevel[] worldConnections;
    [HideInInspector]
    public bool connectionsAvailable = false;

    public void OnDrawGizmos()
    {
        // If in game mode, don't draw gizmos
        if (Application.isPlaying)
        {
            return;
        }

        if (worldConnections == null)
        {
            return;
        }

        // Draw graph
        int minLevel = Mathf.Max(0, minGizmosLevels);
        int maxLevel = Mathf.Clamp(maxGizmosLevels, -1, worldConnections.Length-1);

        // Draw permited levels
        for (int level = minLevel; level <= maxLevel; level++)
        {
            Color color = colors[level % colors.Length];
            WorldConnection[] connections = worldConnections[level].connections;

            foreach (WorldConnection connection in connections)
            {
                // Draw the nodes at level 0
                if (level == 0 && drawSpheres)
                {
                    if (connection.fromTacBenefit > 0)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    else if (connection.fromTacBenefit < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = color;
                    }
                    Gizmos.DrawSphere(connection.from, 0.1f);
                    if (connection.toTacBenefit > 0)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    else if (connection.toTacBenefit < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = color;
                    }
                    Gizmos.DrawSphere(connection.to, 0.1f);
                }
                Gizmos.color = color;

                // Draw the connections
                if (drawConnections)
                {
                    // Draw the connections between the nodes
                    Gizmos.DrawLine(connection.from, connection.to);
                }
                
                // At higher levels, draw the bounding boxes
                if (level != 0 && (drawBoundingBoxes || drawSpheres))
                {
                    // Draw the bounding box of each node at upper levels
                    // Ensure that the bounds are in world space by adding the node's position
                    Vector3 fromBottomLeft = new Vector3(connection.fromRectTransform.rect.xMin, connection.fromRectTransform.rect.yMin, 0) + connection.fromRectTransform.position;
                    Vector3 fromTopLeft = new Vector3(connection.fromRectTransform.rect.xMin, connection.fromRectTransform.rect.yMax, 0) + connection.fromRectTransform.position;
                    Vector3 fromTopRight = new Vector3(connection.fromRectTransform.rect.xMax, connection.fromRectTransform.rect.yMax, 0) + connection.fromRectTransform.position;
                    Vector3 fromBottomRight = new Vector3(connection.fromRectTransform.rect.xMax, connection.fromRectTransform.rect.yMin, 0) + connection.fromRectTransform.position;

                    Vector3 toBottomLeft = new Vector3(connection.toRectTransform.rect.xMin, connection.toRectTransform.rect.yMin, 0) + connection.toRectTransform.position;
                    Vector3 toTopLeft = new Vector3(connection.toRectTransform.rect.xMin, connection.toRectTransform.rect.yMax, 0) + connection.toRectTransform.position;
                    Vector3 toTopRight = new Vector3(connection.toRectTransform.rect.xMax, connection.toRectTransform.rect.yMax, 0) + connection.toRectTransform.position;
                    Vector3 toBottomRight = new Vector3(connection.toRectTransform.rect.xMax, connection.toRectTransform.rect.yMin, 0) + connection.toRectTransform.position;

                    if (drawBoundingBoxes)
                    {
                        // Side lines
                        Gizmos.DrawLine(fromBottomLeft, fromTopLeft);
                        Gizmos.DrawLine(fromTopLeft, fromTopRight);
                        Gizmos.DrawLine(fromTopRight, fromBottomRight);
                        Gizmos.DrawLine(fromBottomRight, fromBottomLeft);

                        Gizmos.DrawLine(toBottomLeft, toTopLeft);
                        Gizmos.DrawLine(toTopLeft, toTopRight);
                        Gizmos.DrawLine(toTopRight, toBottomRight);
                        Gizmos.DrawLine(toBottomRight, toBottomLeft);
                    }

                    if (drawSpheres)
                    {
                        // Draw the spheres at the corners of the bounding box
                        Gizmos.DrawSphere(fromBottomLeft, 0.1f);
                        Gizmos.DrawSphere(fromTopLeft, 0.1f);
                        Gizmos.DrawSphere(fromTopRight, 0.1f);
                        Gizmos.DrawSphere(fromBottomRight, 0.1f);

                        Gizmos.DrawSphere(connection.fromRectTransform.position, 0.1f);

                        Gizmos.DrawSphere(toBottomLeft, 0.1f);
                        Gizmos.DrawSphere(toTopLeft, 0.1f);
                        Gizmos.DrawSphere(toTopRight, 0.1f);
                        Gizmos.DrawSphere(toBottomRight, 0.1f);

                        Gizmos.DrawSphere(connection.toRectTransform.position, 0.1f);
                    }
                }
            }
        }
    }

    public void DrawGraph()
    {
        int minLevel = Mathf.Max(0, minGizmosLevels);
        int maxLevel = Mathf.Clamp(maxGizmosLevels, -1, pfm.hierarchicalGraph.Height()-1);

        // Draw permited levels
        for (int level = minLevel; level <= maxLevel; level++)
        {
            Color color = colors[level % colors.Length];
            Graph graph = pfm.hierarchicalGraph.levels[level];

            foreach (Node node in graph.GetNodes())
            {
                // Draw the connections
                if (drawConnections)
                {
                    foreach (Connection connection in graph.GetConnections(node))
                    {
                        // Draw the connections between the nodes
                        Debug.DrawLine(node.GetPosition(), connection.toNode.GetPosition(), color);
                    }
                }
                
                // At higher levels, draw the bounding boxes
                if (level != 0 && drawBoundingBoxes)
                {
                    // Draw the bounding box of each node at upper levels
                    // Ensure that the bounds are in world space by adding the node's position
                    Vector3 bottomLeft = new Vector3(node.bounds.xMin, node.bounds.yMin, 0) + node.GetPosition();
                    Vector3 topLeft = new Vector3(node.bounds.xMin, node.bounds.yMax, 0) + node.GetPosition();
                    Vector3 topRight = new Vector3(node.bounds.xMax, node.bounds.yMax, 0) + node.GetPosition();
                    Vector3 bottomRight = new Vector3(node.bounds.xMax, node.bounds.yMin, 0) + node.GetPosition();

                    // Side lines
                    Debug.DrawLine(bottomLeft, topLeft, color);
                    Debug.DrawLine(topLeft, topRight, color);
                    Debug.DrawLine(topRight, bottomRight, color);
                    Debug.DrawLine(bottomRight, bottomLeft, color);
                }
            }
        }
    }
    #endregion

    void Start()
    {
        pfm = PathFinderManager.instance;
        // Generate the hierarchical graph based on the children GameObjects
        pfm.hierarchicalGraph = GenerateHierarchicalGraph();
    }

    void Update()
    {
        if (debugInfo)
        {
            DrawGraph();
        }
    }
       
    public void CalculateConnections()
    {
        connectionsAvailable = true;

        // Initialize the connections
        worldConnections = new WorldLevel[transform.childCount+1];

        // Calculate base level in the tilemap
        List<WorldConnection> connectionList = new List<WorldConnection>();
        foreach (Vector3 position in tilemap.cellBounds.allPositionsWithin)
        {
            if (IsWalkableTile(position))
            {
                // Add walkable connections
                Vector3[] directions = { 
                    Vector3.up, Vector3.down, Vector3.left, Vector3.right,
                    Vector3.up + Vector3.left, Vector3.up + Vector3.right,
                    Vector3.down + Vector3.left, Vector3.down + Vector3.right
                };
                foreach (Vector3 direction in directions)
                {
                    Vector3 neighbourPosition = position + direction;
                    if (IsWalkableTile(neighbourPosition))
                    {
                        Node from = new Node(position);
                        Node to = new Node(neighbourPosition);
                        float fromTacBenefit = 0;
                        float toTacBenefit = 0;

                        if (tacticalWaypoints != null)
                        {
                            foreach (Waypoint waypoint in tacticalWaypoints.waypoints)
                            {
                                if (from.Contains(waypoint.position))
                                {
                                    fromTacBenefit += waypoint.tacticalBenefit;
                                }
                                if (to.Contains(waypoint.position))
                                {
                                    toTacBenefit += waypoint.tacticalBenefit;
                                }
                            }
                        }

                        connectionList.Add(new WorldConnection { 
                            from = from.GetPosition(), 
                            to = to.GetPosition(), 
                            fromTacBenefit = fromTacBenefit,
                            toTacBenefit = toTacBenefit
                        });
                    }
                }
            }
        }
        worldConnections[0] = new WorldLevel { connections = connectionList.ToArray() };

        // Iterate over each higher level
        for (int level = 1; level <= transform.childCount; level++)
        {
            // Initialize the list of higher connections for the level
            connectionList = new List<WorldConnection>();

            Transform levelTransform = transform.GetChild(level-1);

            // Iterate over each pair of nodes in the level
            for (int i = 0; i < levelTransform.childCount; i++)
            {
                Transform fromTransform = levelTransform.GetChild(i);
                RectTransform rectFrom = fromTransform.GetComponent<RectTransform>();

                // Get the node from the RectTransform
                Node from = new Node(level);
                from.bounds = rectFrom.rect;
                from.center = rectFrom.position;

                // Make sure the node has a RectTransform component
                if (rectFrom == null) continue;

                // Iterate over the other nodes in the level
                for (int j = i+1; j < levelTransform.childCount; j++)
                {
                    Transform toTransform = levelTransform.GetChild(j);
                    RectTransform rectTo = toTransform.GetComponent<RectTransform>();

                    // Get the node from the RectTransform
                    Node to = new Node(level);
                    to.bounds = rectTo.rect;
                    to.center = rectTo.position;

                    // Make sure the node has a RectTransform component
                    if (rectTo == null || toTransform == fromTransform) continue;

                    // If it's level 1, need to check in the tile graph connections
                    if (level == 1)
                    {
                        foreach (WorldConnection connection in worldConnections[0].connections)
                        {
                            Node fromNode = new Node(connection.from);
                            Node toNode = new Node(connection.to);

                            if (from.Contains(fromNode) && to.Contains(toNode) ||
                                from.Contains(toNode) && to.Contains(fromNode))
                            {
                                connectionList.Add(new WorldConnection { fromRectTransform = rectFrom, toRectTransform = rectTo, from = rectFrom.position, to = rectTo.position });
                                break;
                            }
                        }
                        continue;
                    }

                    // If it's a higher level, check the connections in the previous level
                    foreach (WorldConnection connection in worldConnections[level-1].connections)
                    {
                        Node fromNode = new Node(level-2);
                        fromNode.bounds = connection.fromRectTransform.rect;
                        fromNode.center = connection.fromRectTransform.position;

                        Node toNode = new Node(level-2);
                        toNode.bounds = connection.toRectTransform.rect;
                        toNode.center = connection.toRectTransform.position;

                        if (from.Contains(fromNode) && to.Contains(toNode) ||
                            from.Contains(toNode) && to.Contains(fromNode))
                        {
                            connectionList.Add(new WorldConnection { fromRectTransform = rectFrom, toRectTransform = rectTo, from = rectFrom.position, to = rectTo.position });
                            break;
                        }
                    }
                }
            }

            // Add this level's higher connections to the array
            worldConnections[level] = new WorldLevel { connections = connectionList.ToArray() };
        }
    }

    private Graph GenerateTileGraph() 
    {
        Graph tileGraph = new Graph();

        foreach (WorldConnection connection in worldConnections[0].connections)
        {
            Node fromNode = new Node(connection.from);
            fromNode.tacticalBenefit = connection.fromTacBenefit;
            Node toNode = new Node(connection.to);
            toNode.tacticalBenefit = connection.toTacBenefit;

            tileGraph.AddConnection(fromNode, toNode);
        }

        return tileGraph;
    }

    private HierarchicalGraph GenerateHierarchicalGraph()
    {
        if (pfm.hierarchicalGraph != null) return pfm.hierarchicalGraph;
        
        // If connections have not been calculated, return null
        if (!connectionsAvailable)
        {
            Debug.LogWarning("No connections available. Please calculate connections first.");
            return null;
        }

        HierarchicalGraph hierarchicalGraph = new HierarchicalGraph();

        Graph tileGraph = GenerateTileGraph();
        
        // Add the tile graph as the first level
        hierarchicalGraph.levels.Add(tileGraph);

        // Add the upper levels
        for (int level = 1; level < worldConnections.Length; level++)
        {
            foreach (WorldConnection connection in worldConnections[level].connections)
            {
                Node fromNode = new Node(level);
                fromNode.bounds = connection.fromRectTransform.rect;
                fromNode.center = connection.fromRectTransform.position;

                Node toNode = new Node(level);
                toNode.bounds = connection.toRectTransform.rect;
                toNode.center = connection.toRectTransform.position;

                hierarchicalGraph.AddConnection(level, fromNode, toNode);
            }
        }

        return hierarchicalGraph;
    }

    public bool IsWalkableTile(Vector3 position) 
    {
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile == null) return false;
        
        Sprite sprite = (tile as Tile)?.sprite;
        if (sprite == null) return false;

        string[] parts = sprite.name.Split('_');
        if (parts.Length > 1 && int.TryParse(parts[1], out int id) && walkableTiles.Contains(id)) 
        {
            return true;
        }

        return false;
    }
}