using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3Int[,] nodes;
    AStar astar;
    List<Node> roadPath = new List<Node>();
    new Camera camera;
    BoundsInt bounds;
    Vector2Int start;
    Vector2Int end;
    public List<GameObject> waypointList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Shrink bounds down to where there are tiles
        tilemap.CompressBounds();
        // Returns bounds
        bounds = tilemap.cellBounds;

        CreateGrid();
        // Creates instance of A*
        astar = new AStar(nodes, bounds.size.x, bounds.size.y);

        start = new Vector2Int((int)transform.position.x, (int)transform.position.z);
        end = new Vector2Int((int)waypointList[0].transform.position.x, (int)waypointList[0].transform.position.z);
    }

    // Creates grid
    public void CreateGrid()
    {
        // 2D array of whether or not there is a tile
        nodes = new Vector3Int[bounds.size.x, bounds.size.y];
        // Search whole area
        for(int x = bounds.xMin, i = 0; i < (bounds.size.x); x++, i++)
        {
            for(int y = bounds.yMin, j = 0; j < (bounds.size.y); y++, j++)
            {
                if(tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    nodes[i, j] = new Vector3Int(x, y, 0); // Has tile
                }
                else
                {
                    nodes[i, j] = new Vector3Int(x, y, 1); // Doesn't have tile
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2((int)waypointList[0].transform.position.x, (int)waypointList[0].transform.position.z)));
        if(roadPath != null && roadPath.Count > 0 && waypointList.Count > 0)
        {
            // Move towards the next node
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y), 12 * Time.deltaTime);

            Vector3 temp = new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y);

            // Switches to next node on path
            if(Vector3.Distance(transform.position, temp) < 0.1f)
            {
                start = new Vector2Int(roadPath[0].X, roadPath[0].Y);
                roadPath.RemoveAt(0);
            }
            if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2((int)waypointList[0].transform.position.x, (int)waypointList[0].transform.position.z)) < 0.1f)
            {
                if(waypointList[0].transform.position.y > 5)
                {
                    int randomPath = Random.Range(0, 2);
                    if(randomPath == 0)
                    {
                        waypointList.RemoveAt(2);
                        waypointList.RemoveAt(0);
                    }
                    else
                    {
                        waypointList.RemoveAt(1);
                        waypointList.RemoveAt(0);
                    }
                }
                else
                {
                    waypointList.RemoveAt(0);
                }
                start = new Vector2Int((int)transform.position.x, (int)transform.position.z);
                if(waypointList.Count > 0)
                {
                    end = new Vector2Int((int)waypointList[0].transform.position.x, (int)waypointList[0].transform.position.z);
                }
                roadPath.Clear();
            }
        }
        else
        {
            if(roadPath == null || roadPath.Count == 0)
            {
                // Calls the A* algorithm
                roadPath = astar.CreatePath(nodes, start, end);
            }
        }
    }
}