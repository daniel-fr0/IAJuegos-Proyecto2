using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

public class TileClassifier : MonoBehaviour
{
    private Tilemap tilemap;

    // Walkable tile ids
	public int[] walkableTiles = {0,1,8,9,11};

    [Serializable]
    public class NodeGroup
    {
        public Vector2 fromNode;
        public Vector2 toNode;
    }

    [Serializable]
    public class GraphLevel
    {
        public NodeGroup[] groups;
    }
    
    // Levels for hierarchical graph
    // each level is defined by two coordinates: north-west and south-east
    public GraphLevel[] levels;

    public bool debugInfo = false;

    void Start()
    {
        // Get the Tilemap component
        tilemap = GetComponent<Tilemap>();

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
        }
    }

    private void GenerateTileGraph() 
    {
        Graph tileGraph = new Graph();
        PathFinder.instance.tileGraph = tileGraph;

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

    private bool IsWalkableTile(Vector3 position) 
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
    }

    public void DrawGroups()
    {
        // Draw the levels
        for (int level = 0; level < levels.Length; level++)
        {
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
}