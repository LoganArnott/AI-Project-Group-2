using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public Node[,] nodes;
    
    public AStar(Vector3Int[,] grid, int columns, int rows)
    {
        nodes = new Node[columns, rows];
    }

    // Creates A* path
    public List<Node> CreatePath(Vector3Int[,] grid, Vector2Int start, Vector2Int end)
    {
        Node End = null;
        Node Start = null;
        int columns = nodes.GetUpperBound(0) + 1;
        int rows = nodes.GetUpperBound(1) + 1;
        nodes = new Node[columns, rows];

        // Translates the given Vector3 values from the grid into nodes
        for(int i = 0; i < columns; i++)
        {
            for(int j = 0; j < rows; j++)
            {
                if(grid[i, j].z == 0)
                {
                    nodes[i, j] = new Node(grid[i, j].x, grid[i, j].y); // Translate grid into nodes
                }
                else
                {
                    nodes[i, j] = null;
                }
            }
        }

        // Sets the available neighbors for each node and sets our start & end nodes
        for(int i = 0; i < columns; i++)
        {
            for(int j = 0; j < rows; j++)
            {
                if(nodes[i, j] != null)
                {
                    nodes[i, j].AddNeighbors(nodes, i, j);
                    if(nodes[i, j].X == start.x && nodes[i, j].Y == start.y)
                    {
                        Start = nodes[i, j]; // Sets start
                    }
                    else if(nodes[i, j].X == end.x && nodes[i, j].Y == end.y)
                    {
                        End = nodes[i, j]; // Sets end
                    }
                }
            }
        }
        List<Node> openList = new List<Node>(); // Possible nodes to visit
        List<Node> closedList = new List<Node>(); // Already visited nodes

        openList.Add(Start);

        while(openList.Count > 0)
        {
            // Find shortest distance towards the goal within openList
            int shortestF = 0;
            for(int i = 0; i < openList.Count; i++)
            {
                if(openList[i].F < openList[shortestF].F)
                {
                    shortestF = i;
                }
            }

            Node current = openList[shortestF];

            // Reached the end and creates a path out of all the quickest nodes to the end
            if(openList[shortestF] == End)
            {
                List<Node> path = new List<Node>();
                Node temp = current;
                path.Add(temp);
                while(temp.parent != null)
                {
                    path.Add(temp.parent);
                    temp = temp.parent;
                }
                path.Reverse();
                return path;
            }

            openList.Remove(current); // Removes searched node
            closedList.Add(current); // Adds searched node


            // Look at all the neighbors of the current node
            List<Node> neighbors = current.Neighbors;
            for(int i = 0; i < neighbors.Count; i++)
            {
                Node n = neighbors[i];
                if(!closedList.Contains(n)) // Makes sure neighbor isn't in closedList
                {
                    float tempG = current.G + 1f;

                    if(tempG < n.G || !openList.Contains(n))
                    {
                        n.G = tempG;
                        if(End != null)
                        {
                             n.H = Heuristic(n, End);
                        }
                        n.F = n.G + n.H;
                        n.parent = current;

                        if(!openList.Contains(n))
                        {
                            openList.Add(n);
                        }
                    }
                }
            }
        }
        return null;
    }

    private float Heuristic(Node a, Node b)
    {
        // octile distance
        float D = 1;
        float D2 = Mathf.Sqrt(2);
        float dx = Math.Abs(a.X - b.X);
        float dy = Math.Abs(a.Y - b.Y);
        
        return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
    }
}