using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Behaviors {Idle, Attack, SwitchTarget};

public class SingleAgent : MonoBehaviour
{
    // Finite state machine variables
    public Behaviors aiBehaviors = Behaviors.Idle;
    RadiusDetection childScript;
    GameObject target;

    // Gridmap A* variables
    public Tilemap tilemap;
    public Vector3Int[,] nodes;
    AStar astar;
    List<Node> roadPath = new List<Node>();
    BoundsInt bounds;
    Vector2Int startPos;
    Vector2Int start;
    Vector2Int end;
    public float speed = 20;
    bool attackingCoroutine = true;
    bool idleMove = true;
    public float waitSeconds = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        // Gets script from its child
        childScript = GetComponentInChildren<RadiusDetection>();

        // Shrink bounds down to where there are tiles
        tilemap.CompressBounds();
        // Returns bounds
        bounds = tilemap.cellBounds;

        CreateGrid();
        // Creates instance of A*
        astar = new AStar(nodes, bounds.size.x, bounds.size.y);
    }

    // Update is called once per frame
    void Update()
    {
        RunBehaviors();
    }

    // Switch statement for behaviors
    void RunBehaviors()
	{
		switch(aiBehaviors)
		{
		case Behaviors.Idle:
			Idle();
			break;
		case Behaviors.Attack:
			Attack();
			break;
        case Behaviors.SwitchTarget:
			SwitchTarget();
			break;
		}
	}

    // Changes behavior in enum
    void ChangeBehavior(Behaviors newBehavior)
	{
		aiBehaviors = newBehavior;

		RunBehaviors();
	}

    // Return to startPos and wait until racer enters radius
    void Idle()
    {
        start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        end = startPos;

        // Reset A* pathing once
        if(idleMove)
        {
            attackingCoroutine = true;
            StartCoroutine(ResetAStarTarget());
            StopCoroutine(ResetAStarTarget());
            idleMove = false;
            attackingCoroutine = false;
        }
        Movement();


        if(childScript.racersInRadius.Count > 0)
        {
            target = childScript.racersInRadius[0];
            if(roadPath != null)
            {
                roadPath.Clear();
            }
            attackingCoroutine = true;
            StartCoroutine(ResetAStarTarget());
            ChangeBehavior(Behaviors.Attack);
        }
    }

    // A* path to first racer to enter radius
    void Attack()
    {
        start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        end = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z));
        Movement();

        // No racers in radius
        if(childScript.racersInRadius.Count == 0)
        {
            StopCoroutine(ResetAStarTarget());
            if(roadPath != null)
            {
                roadPath.Clear();
            }
            idleMove = true;
            attackingCoroutine = false;
            ChangeBehavior(Behaviors.Idle);
        }
        else
        {
            // Always target most recent racer in radius (if first racer exits radius, it'll target the second racer who entered the radius)
            target = childScript.racersInRadius[0];
        }
    }

    // If it touches a racer, target another racer
    void SwitchTarget()
    {
        childScript.racersInRadius.RemoveAt(0);

        // If no more racers in radius switch to idle
        if(childScript.racersInRadius.Count == 0)
        {
            StopCoroutine(ResetAStarTarget());
            idleMove = true;
            attackingCoroutine = false;
            ChangeBehavior(Behaviors.Idle);
        }
        else
        {
            ChangeBehavior(Behaviors.Attack);
        }
    }

    // If single agent touches a racer
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject == target)
        {
            ChangeBehavior(Behaviors.SwitchTarget);
        }
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

    // Movement method that uses A*
    void Movement()
    {
        if(roadPath != null && roadPath.Count > 0)
        {
            // Move towards the next node
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y), speed * Time.deltaTime);

            Vector3 temp = new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y);

            // Switches to next node on path
            if(Vector3.Distance(transform.position, temp) < 0.1f)
            {
                roadPath.RemoveAt(0);
            }
        }
    }

    // Coroutine to reset A* path every "waitSeconds" seconds which allow single agent to keep pathing towards racer
    IEnumerator ResetAStarTarget()
    {
        while(attackingCoroutine)
        {
            if(roadPath != null)
            {
                roadPath.Clear();
            }
            roadPath = astar.CreatePath(nodes, start, end);
            yield return new WaitForSeconds(waitSeconds);
        }
    }
}
