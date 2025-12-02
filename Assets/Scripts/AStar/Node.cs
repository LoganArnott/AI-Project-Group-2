using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int X;
    public int Y;
    public float F;
    public float G;
    public float H;

    public List<Node> Neighbors;
    public Node parent = null;

    public Node(int x, int y)
    {
        X = x;
        Y = y;
        F = 0f;
        G = 0f;
        H = 0f;
        Neighbors = new List<Node>();
    }

    public void AddNeighbors(Node[,] grid, int x, int y)
    {
        // Checks left, right, up, down
        if(x < grid.GetUpperBound(0) && grid[x + 1, y] != null)
        {
            Neighbors.Add(grid[x + 1, y]);
        }
        if(x > 0 && grid[x - 1, y] != null)
        {
            Neighbors.Add(grid[x - 1, y]);
        }
        if(y < grid.GetUpperBound(1) && grid[x, y + 1] != null)
        {
            Neighbors.Add(grid[x, y + 1]);
        }
        if(y > 0 && grid[x, y - 1] != null)
        {
            Neighbors.Add(grid[x, y - 1]);
        }

        // Checks diagonal
        if(x > 0 && y > 0 && grid[x - 1, y - 1] != null)
        {
           Neighbors.Add(grid[x - 1, y - 1]);
        }
        if(x < grid.GetUpperBound(0) && y > 0 && grid[x + 1, y - 1] != null)
        {
           Neighbors.Add(grid[x + 1, y - 1]);
        }
        if(x > 0 && y < grid.GetUpperBound(1) && grid[x - 1, y + 1] != null)
        {
           Neighbors.Add(grid[x - 1, y + 1]);
        }
        if(x < grid.GetUpperBound(0) && y < grid.GetUpperBound(1) && grid[x + 1, y + 1] != null)
        {
           Neighbors.Add(grid[x + 1, y + 1]);
        }
    }


}
