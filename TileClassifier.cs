using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using System.Collections.Generic;

public class TileClassifier : MonoBehaviour
{
    public Tilemap tilemap;

    private Graph tileGraph;
    public HierarchicalGraph hierarchicalGraph;

    // Walkable tile ids
	public int[] walkableTiles = {0,1,8,9,11};
    
    // Levels for hierarchical graph
    public int minGizmosLevels = 0;
    public int maxGizmosLevels = 0;
    public bool debugInfo = false;

    #region Input System + Singleton Initialization
    private InputSystem_Actions controls;
    public static TileClassifier instance;

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Toggle debug info
        controls.DebugUI.ToggleMap.performed += ctx => debugInfo = !debugInfo;

        // Increase/decrease the number of levels shown
        controls.DebugUI.DecreaseMinLevel.performed += ctx => minGizmosLevels = Mathf.Clamp(minGizmosLevels-1, 0, maxGizmosLevels);
        controls.DebugUI.IncreaseMinLevel.performed += ctx => minGizmosLevels = Mathf.Clamp(minGizmosLevels+1, 0, maxGizmosLevels);

        controls.DebugUI.DecreaseMaxLevel.performed += ctx => maxGizmosLevels = Mathf.Clamp(maxGizmosLevels-1, minGizmosLevels, hierarchicalGraph.Height());
        controls.DebugUI.IncreaseMaxLevel.performed += ctx => maxGizmosLevels = Mathf.Clamp(maxGizmosLevels+1, minGizmosLevels, hierarchicalGraph.Height());

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
    public bool drawBoundingBoxes = true;
    public bool drawConnections = true;
    public bool DrawSpheres = true;
    private Color[] colors = { Color.green, Color.red, Color.cyan, Color.magenta, Color.yellow, Color.blue, Color.white, Color.black };
    public void OnDrawGizmos()
    {
        // If in game mode, don't draw path
        if (Application.isPlaying)
        {
            return;
        }

        GenerateTileGraph();
        GenerateHierarchicalGraph();
        DrawGizmos();
    }

    public void DrawGizmos()
    {
        // Draw each level
        int level = -1;
        foreach (Graph graph in hierarchicalGraph.levels)
        {
            level++;
            if (level < minGizmosLevels || maxGizmosLevels < level)
            {
                continue;
            }

            foreach (Node node in graph.GetNodes())
            {
                Gizmos.color = colors[level % colors.Length];
                // At level 0 just draw the connections and sphere
                if (level == 0)
                {
                    if (DrawSpheres)  Gizmos.DrawSphere(node.GetPosition(), 0.1f);
                    
                    foreach (Connection connection in graph.GetConnections(node))
                    {
                        Gizmos.DrawLine(node.GetPosition(), connection.toNode.GetPosition());
                    }
                    continue;
                }
                
                // Draw the bounding box of each node at upper levels
                // Ensure that the bounds are in world space by adding the node's position
                Vector3 bottomLeft = new Vector3(node.bounds.xMin, node.bounds.yMin, 0) + node.GetPosition();
                Vector3 topLeft = new Vector3(node.bounds.xMin, node.bounds.yMax, 0) + node.GetPosition();
                Vector3 topRight = new Vector3(node.bounds.xMax, node.bounds.yMax, 0) + node.GetPosition();
                Vector3 bottomRight = new Vector3(node.bounds.xMax, node.bounds.yMin, 0) + node.GetPosition();

                if (drawBoundingBoxes)
                {
                    // Side lines
                    Gizmos.DrawLine(bottomLeft, topLeft);
                    Gizmos.DrawLine(topLeft, topRight);
                    Gizmos.DrawLine(topRight, bottomRight);
                    Gizmos.DrawLine(bottomRight, bottomLeft);

                    if (DrawSpheres)
                    {
                        // Center of the node
                        Gizmos.DrawSphere(node.GetPosition(), 0.1f);

                        // Vertices
                        Gizmos.DrawSphere(bottomLeft, 0.1f);
                        Gizmos.DrawSphere(topLeft, 0.1f);
                        Gizmos.DrawSphere(topRight, 0.1f);
                        Gizmos.DrawSphere(bottomRight, 0.1f);
                    }
                }

                // Draw the connections between the nodes
                if (!drawConnections) continue;
                foreach (Connection connection in graph.GetConnections(node))
                {
                    Gizmos.DrawLine(node.GetPosition(), connection.toNode.GetPosition());
                }
            }
        }
    }

    public void DrawGraph()
    {
        // Draw each level
        int level = -1;
        foreach (Graph graph in hierarchicalGraph.levels)
        {
            level++;
            if (level < minGizmosLevels || maxGizmosLevels < level)
            {
                continue;
            }

            Color color = colors[level % colors.Length];
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
        // Generate the tile graph
        PathFinderManager.instance.tileGraph = GenerateTileGraph();

        // Generate the hierarchical graph based on the children GameObjects
        PathFinderManager.instance.hierarchicalGraph = GenerateHierarchicalGraph();
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
        if (tileGraph != null) return tileGraph;
        tileGraph = new Graph();

        foreach (Vector3 position in tilemap.cellBounds.allPositionsWithin) 
        {
            if (IsWalkableTile(position))
            {
                Node node = new Node(position);

                // Add walkable connections
                Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
                foreach (Vector3 direction in directions) 
                {
                    Vector3 neighbourPosition = position + direction;
                    if (IsWalkableTile(neighbourPosition)) 
                    {
                        Node neighbourNode = new Node(neighbourPosition);
                        tileGraph.AddConnection(node, neighbourNode);
                    }
                }
            }            
        }

        return tileGraph;
    }

    private HierarchicalGraph GenerateHierarchicalGraph()
    {
        if (hierarchicalGraph != null) return hierarchicalGraph;
        hierarchicalGraph = new HierarchicalGraph();

        if (tileGraph == null) GenerateTileGraph();
        
        // Add the tile graph as the first level
        hierarchicalGraph.levels.Add(tileGraph);

        // Add the upper levels
        int level = 1;

        // The children of the TileClassifier GameObject are the levels
        foreach (Transform levelTransform in transform)
        {
            hierarchicalGraph.levels.Add(new Graph());

            List<Node> nodes = new List<Node>();

            // Inside each level, the children are the nodes
            foreach (Transform child in levelTransform)
            {
                // The RectTransform defines the bounds
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                
                // Create a new upper node
                Node node = new Node(level);

                // Set the bounds and center of the upper node
                node.bounds = rectTransform.rect;
                node.center = rectTransform.position;

                // Add the node to the nodes list
                nodes.Add(node);
            }

            // Find the connections between the nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i+1; j < nodes.Count; j++)
                {
                    Node fromNode = nodes[i];
                    Node toNode = nodes[j];

                    // Iterate at the lower level to find the connection
                    bool hasConnection = false;
                    foreach (Node lowerNode in hierarchicalGraph.levels[level-1].GetNodes())
                    {
                        foreach (Connection connection in hierarchicalGraph.levels[level-1].GetConnections(lowerNode))
                        {
                            if (fromNode.Contains(connection.fromNode.GetPosition()) && toNode.Contains(connection.toNode.GetPosition()) ||
                                fromNode.Contains(connection.toNode.GetPosition()) && toNode.Contains(connection.fromNode.GetPosition()))
                            {
                                hierarchicalGraph.AddConnection(level, fromNode, toNode);
                                hasConnection = true;
                                break;
                            }
                        }
                        if (hasConnection) break;
                    }
                }
            }

            level++;
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