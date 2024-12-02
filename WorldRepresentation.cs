using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(WorldRepresentation))]
public class WorldRepresentationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldRepresentation wrld = (WorldRepresentation)target;
        if (GUILayout.Button("Calculate Connections"))
        {
            wrld.CalculateConnections();
            EditorUtility.SetDirty(wrld);
            Debug.Log("Connections calculated!");
        }

        if (GUILayout.Button("Clear Connections"))
        {
            wrld.connectionsAvailable = false;
            wrld.worldConnections = null;
            EditorUtility.SetDirty(wrld);
            Debug.Log("Connections cleared!");
        }

        if (GUILayout.Button("Check for duplicates"))
        {
            foreach (Transform levelTransform in wrld.transform)
            {
                for (int i = 0; i < levelTransform.childCount; i++)
                {
                    Transform nodeTransform = levelTransform.GetChild(i);

                    for (int j = i+1; j < levelTransform.childCount; j++)
                    {
                        Transform otherTransform = levelTransform.GetChild(j);

                        if (nodeTransform.position.Equals(otherTransform.position))
                        {
                            Debug.LogWarning("Overlapping nodes found!");
                            Debug.LogWarning("Node 1: " + nodeTransform.name);
                            Debug.LogWarning("Node 2: " + otherTransform.name);
                        }
                    }
                }
            }
            Debug.Log("Duplicates checked!");
        }
    }
}

public class WorldRepresentation : MonoBehaviour
{
    public Tilemap tilemap;
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

        // Draw nodes
        DrawNodes();

        // Draw connections
        DrawConnections();
    }

    private void DrawTileMapNodes()
    {
        foreach (Vector3 position in tilemap.cellBounds.allPositionsWithin)
        {
            if (IsWalkableTile(position))
            {
                Gizmos.DrawSphere(position + new Vector3(0.5f, 0.5f), 0.1f);
            }
        }
    }

    public void DrawNodes()
    {
        // If everything is disabled, don't draw anything
        if (!drawBoundingBoxes && !drawSpheres) return;

        int minLevel = Mathf.Max(0, minGizmosLevels);
        int maxLevel = Mathf.Clamp(maxGizmosLevels, -1, transform.childCount);

        // Draw the levels specified by the min and max levels
        for (int level = minLevel; level <= maxLevel; level++)
        {
            // Each level has a different color
            Gizmos.color = colors[level % colors.Length];

            if (level == 0)
            {
                if (tilemap != null && drawSpheres)
                {
                    // Draw tilemap nodes
                    DrawTileMapNodes();
                }
                // Skip to the next iteration to avoid accessing transform.GetChild(-1)
                continue;
            }

            // Skip if bounding boxes are disabled
            if (!drawBoundingBoxes) break;

            // At upper levels, draw the nodes from the RectTransforms
            Transform levelTransform = transform.GetChild(level-1);
            foreach (Transform nodeTransform in levelTransform)
            {
                RectTransform rectTransform = nodeTransform.GetComponent<RectTransform>();
                if (rectTransform == null) continue;

                // For the bounding box, we need the corners of the rect in world space
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                Gizmos.DrawLine(corners[0], corners[1]);
                Gizmos.DrawLine(corners[1], corners[2]);
                Gizmos.DrawLine(corners[2], corners[3]);
                Gizmos.DrawLine(corners[3], corners[0]);

                if (drawSpheres)
                {
                    Gizmos.DrawSphere(corners[0], 0.1f);
                    Gizmos.DrawSphere(corners[1], 0.1f);
                    Gizmos.DrawSphere(corners[2], 0.1f);
                    Gizmos.DrawSphere(corners[3], 0.1f);

                    Gizmos.DrawSphere(rectTransform.position, 0.1f);
                }
            }
        }
    }

    private void DrawConnections()
    {
        if (!drawConnections || !connectionsAvailable) return;

        // Draw the saved connections
        for (int level = 0; level < worldConnections.Length; level++)
        {
            if (level < minGizmosLevels || maxGizmosLevels < level) continue;

            WorldConnection[] connections = worldConnections[level].connections;
            if (connections == null) continue;

            Gizmos.color = colors[level % colors.Length];
            foreach (WorldConnection connection in connections)
            {
                Gizmos.DrawLine(connection.from, connection.to);
            }
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
                        Vector3 from = position + new Vector3(0.5f, 0.5f);
                        Vector3 to = neighbourPosition + new Vector3(0.5f, 0.5f);
                        connectionList.Add(new WorldConnection { from = from, to = to });
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
                // At level 0 just draw the connections and sphere
                if (level == 0)
                {
                    foreach (Connection connection in graph.GetConnections(node))
                    {
                        // Draw the connections between the nodes
                        Debug.DrawLine(node.GetPosition(), connection.toNode.GetPosition(), color);
                    }
                    continue;
                }
                
                if (drawBoundingBoxes)
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

                // Draw the connections between the nodes
                if (!drawConnections) continue;
                foreach (Connection connection in graph.GetConnections(node))
                {
                    Debug.DrawLine(node.GetPosition(), connection.toNode.GetPosition(), color);
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

    private Graph GenerateTileGraph() 
    {
        Graph tileGraph = new Graph();

        foreach (WorldConnection connection in worldConnections[0].connections)
        {
            Node fromNode = new Node(connection.from);
            Node toNode = new Node(connection.to);

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