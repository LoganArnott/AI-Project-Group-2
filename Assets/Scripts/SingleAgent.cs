using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Behaviors {Idle, Attack, SwitchTarget};

public class SingleAgent : MonoBehaviour
{
    public Behaviors aiBehaviors = Behaviors.Idle;
    RadiusDetection childScript;
    GameObject target;

    // Gridmap A* variables
    public Tilemap tilemap;
    public Vector3Int[,] nodes;
    AStar astar;
    List<Node> roadPath = new List<Node>();
    new Camera camera;
    BoundsInt bounds;
    Vector2Int start;
    Vector2Int end;
    public float speed = 20;
    bool attackingCoroutine = true;
    Vector2Int startPos;
    bool idleMove = true;

    // Start is called before the first frame update
    void Start()
    {
        startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
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

    void ChangeBehavior(Behaviors newBehavior)
	{
		aiBehaviors = newBehavior;

		RunBehaviors();
	}

    void Idle()
    {
        start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        end = startPos;
        if(idleMove)
        {
            StartCoroutine(ResetAStarTarget());
            StopCoroutine(ResetAStarTarget());
            idleMove = false;
        }
        Movement();


        if(childScript.racersInRadius.Count > 0)
        {
            target = childScript.racersInRadius[0];
            StartCoroutine(ResetAStarTarget());
            ChangeBehavior(Behaviors.Attack);
        }
    }

    void Attack()
    {
        // transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z), 5 * Time.deltaTime);
        start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        end = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z));
        Movement();
        if(childScript.racersInRadius.Count == 0)
        {
            StopCoroutine(ResetAStarTarget());
            idleMove = true;
            ChangeBehavior(Behaviors.Idle);
        }
    }

    void SwitchTarget()
    {
        childScript.racersInRadius.RemoveAt(0);
        if(childScript.racersInRadius.Count == 0)
        {
            StopCoroutine(ResetAStarTarget());
            idleMove = true;
            ChangeBehavior(Behaviors.Idle);
        }
        else
        {
            ChangeBehavior(Behaviors.Attack);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // Send message slow down
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
                // start = new Vector2Int(roadPath[0].X, roadPath[0].Y);
                roadPath.RemoveAt(0);
            }
            if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z))) < 0.1f)
            {
                roadPath.Clear();
            }
        }
    }

    IEnumerator ResetAStarTarget()
    {
        while(attackingCoroutine)
        {
            if(roadPath != null)
            {
                roadPath.Clear();
            }
            roadPath = astar.CreatePath(nodes, start, end);
            yield return new WaitForSeconds(.2f);
        }
    }
}
