using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

public class TileClassifier : MonoBehaviour
{
    public Tilemap tilemap;

    private Graph tileGraph;

    // Walkable tile ids
	public int[] walkableTiles = {0,1,8,9,11};
    
    // Levels for hierarchical graph
    public GraphLevel[] levels;
    public int gizmosLevels = 0;
    public bool debugInfo = false;

    #region Gizmos and Debug + Input System + Singleton Initialization
    private InputSystem_Actions controls;
    public static TileClassifier instance;

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Toggle debug info
        controls.UI.ToggleMap.performed += ctx => debugInfo = !debugInfo;

        // Increase/decrease the number of levels shown
        controls.Player.Previous.performed += ctx => gizmosLevels = (int)Mathf.Repeat(gizmosLevels-1, levels.Length+1);
        controls.Player.Next.performed += ctx => gizmosLevels = (int)Mathf.Repeat(gizmosLevels+1, levels.Length+1);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void OnDrawGizmos()
    {
        // If in game mode, don't draw path
        if (Application.isPlaying)
        {
            return;
        }

        // Draw the levels
        for (int level = 0; level < transform.childCount; level++)
        {
            if (level+1 > gizmosLevels)
            {
                continue;
            }
            // Color is based on the level, cycling through the rainbow
            Gizmos.color = Color.HSVToRGB((float)level / transform.childCount, 1, 1);

            Transform levelTransform = transform.GetChild(level);

            // Draw the groups of the current level
            for (int i = 0; i < levelTransform.childCount; i++)
            {
                RectTransform groupTransform = levelTransform.GetChild(i).GetComponent<RectTransform>();

                // Get the corners of the group
                Vector3[] corners = new Vector3[4];
                groupTransform.GetWorldCorners(corners);

                Vector3 bottomLeft = corners[0];
                Vector3 topLeft = corners[1];
                Vector3 topRight = corners[2];
                Vector3 bottomRight = corners[3];

                // Draw the bounding box
                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);

                // Add a sphere at the vertices
                Gizmos.DrawSphere(bottomLeft, 0.1f);
                Gizmos.DrawSphere(topLeft, 0.1f);
                Gizmos.DrawSphere(topRight, 0.1f);
                Gizmos.DrawSphere(bottomRight, 0.1f);
            }
        }

        // Draw the walkable tiles
        foreach (Vector3 position in tilemap.cellBounds.allPositionsWithin)
        {
            if (gizmosLevels < 0) break;

            if (IsWalkableTile(position))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(position+Vector3.one * 0.5f, 0.1f);

                Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
                foreach (Vector3 direction in directions)
                {
                    Vector3 neighbourPosition = position + direction;
                    if (IsWalkableTile(neighbourPosition))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(position+Vector3.one * 0.5f, neighbourPosition+Vector3.one * 0.5f);
                    }
                }
            }
        }
    }

    public void DrawGroups()
    {
        // Draw the levels
        for (int level = 0; level < levels.Length; level++)
        {
            if (level+1 > gizmosLevels)
            {
                continue;
            }
            // Color is based on the level, cycling through the rainbow
            Color color = Color.HSVToRGB((float)level / levels.Length, 1, 1);

            // Draw the groups of the current level
            for (int i = 0; i < levels[level].groups.Length; i++)
            {
                NodeGroup group = levels[level].groups[i];

                // Get the corners of the group
                Vector3 bottomLeft = new Vector3(group.fromNode.x, group.toNode.y, 0);
                Vector3 topLeft = new Vector3(group.fromNode.x, group.fromNode.y, 0);
                Vector3 topRight = new Vector3(group.toNode.x, group.fromNode.y, 0);
                Vector3 bottomRight = new Vector3(group.toNode.x, group.toNode.y, 0);

                // Draw the bounding box
                Debug.DrawLine(bottomLeft, topLeft, color);
                Debug.DrawLine(topLeft, topRight, color);
                Debug.DrawLine(topRight, bottomRight, color);
                Debug.DrawLine(bottomRight, bottomLeft, color);
            }
        }
    }

    public void DrawTiles()
    {
        if (gizmosLevels < 0) return;
        foreach (Node node in tileGraph.GetNodes())
        {
            foreach (Connection connection in tileGraph.GetConnections(node))
            {
                Debug.DrawLine(node.GetPosition(), connection.toNode.GetPosition(), Color.green);
            }
        }
    }
    #endregion

    void Start()
    {
        // Generate the tile graph
        GenerateTileGraph();

        // Generate the hierarchical graph based on the children GameObjects
        GenerateGraphGroups();
    }

    void Update()
    {
        if (debugInfo)
        {
            DrawGroups();
            DrawTiles();
        }
    }

    private void GenerateTileGraph() 
    {
        tileGraph = new Graph();
        PathFinderManager.instance.graph = tileGraph;

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

    private void GenerateGraphGroups() {
        levels = new GraphLevel[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform levelTransform = transform.GetChild(i);
            GraphLevel level = new GraphLevel();
            levels[i] = level;

            level.groups = new NodeGroup[levelTransform.childCount];
            for (int j = 0; j < levelTransform.childCount; j++)
            {
                RectTransform groupTransform = levelTransform.GetChild(j).GetComponent<RectTransform>();
                NodeGroup group = new NodeGroup();
                level.groups[j] = group;

                // Get the corners of the group
                Vector3[] corners = new Vector3[4];
                groupTransform.GetWorldCorners(corners);

                group.fromNode = new Vector2(corners[1].x, corners[1].y); // Top left
                group.toNode = new Vector2(corners[3].x, corners[3].y); // Bottom right
            }
        }
        PathFinderManager.instance.graphLevels = levels;
    }
}