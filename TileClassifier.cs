using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TileClassifier : MonoBehaviour
{
    private Tilemap tilemap;

    // Walkable tile ids
	public int[] walkableTiles = {0,1,8,9,11};

    void Awake()
    {
        // Get the Tilemap component
        tilemap = GetComponent<Tilemap>();

        // Generate the tile graph
        GenerateTileGraph();
    }

    void GenerateTileGraph() 
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
}