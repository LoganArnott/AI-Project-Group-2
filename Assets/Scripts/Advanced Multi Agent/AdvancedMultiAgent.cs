using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RacerBehaviors {GoToGoal, UseItem, SpinOut};

public class AdvancedMultiAgent : MonoBehaviour
{
    // Finite state machine variables
    public RacerBehaviors aiBehaviors = RacerBehaviors.GoToGoal;

    public Tilemap tilemap;
    public Vector3Int[,] nodes;
    AStar astar;
    List<Node> roadPath = new List<Node>();
    BoundsInt bounds;
    Vector2Int start;
    Vector2Int end;
    public List<GameObject> waypointList = new List<GameObject>();
    public float speed = 5;
    public GameObject placementGameObject;
    Placements placementScript;
    int currentPlacement;
    int savedPlacement;

    public GameObject teammate1;
    public GameObject teammate2;
    bool fork = false;
    int forksGoneThrough = 0;
    bool skipFork = false;
    GameObject currentForkWaypoint;
    GameObject otherForkWaypoint;

    Collider m_ObjectCollider;
    bool checkBehind = true;
    float rotateRacer = 0f;
    bool alreadyHelping = false;
    bool speedUpTimer = false;
    bool slowDownTimer = false;

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

        start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        end = new Vector2Int(Mathf.RoundToInt(waypointList[0].transform.position.x), Mathf.RoundToInt(waypointList[0].transform.position.z));
        
        placementScript = placementGameObject.GetComponent<Placements>();

        m_ObjectCollider = GetComponent<Collider>();
        
        StartCoroutine(CheckWhosBehind());

        tempSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        RunBehaviors();
        RacerPosition();
    }

    // Switch statement for behaviors
    void RunBehaviors()
	{
		switch(aiBehaviors)
		{
		case RacerBehaviors.GoToGoal:
			GoToGoal();
			break;
		case RacerBehaviors.UseItem:
            StopCoroutine(CheckWhosBehind());
			UseItem();
			break;
        case RacerBehaviors.SpinOut:
			SpinOut();
			break;
        // case RacerBehaviors.AllyEnemyAlly:
		// 	AllyEnemyAlly();
		// 	break;
		}
	}

    // Changes behavior in enum
    void ChangeBehavior(RacerBehaviors newBehavior)
	{
		aiBehaviors = newBehavior;

		RunBehaviors();
	}

    #region GoToGoal

    // GoToGoal Behavior
    void GoToGoal()
    {
        if(waypointList.Count > 0)
        {
            Movement();
        }
    }
    
    IEnumerator CheckWhosBehind()
    {
        while(checkBehind)
        {
            if(currentPlacement < placementScript.racerListOrdered.Count - 1 &&
               (placementScript.racerListOrdered[currentPlacement + 1].Racer == teammate1 || placementScript.racerListOrdered[currentPlacement + 1].Racer == teammate2) &&
               (placementScript.racerListOrdered[currentPlacement].Racer != teammate1 && placementScript.racerListOrdered[currentPlacement].Racer != teammate2))
            {
                placementScript.racerListOrdered[currentPlacement + 1].Racer.SendMessage("Help");
            }
            yield return new WaitForSeconds(1f);
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

    // Movement with A*
    void Movement()
    {
        if(roadPath != null && roadPath.Count > 0 && waypointList.Count > 0)
        {
            // Move towards the next node
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y), speed * Time.deltaTime);

            Vector3 temp = new Vector3(roadPath[0].X, this.transform.position.y, roadPath[0].Y);

            // Update distance to checkpoint in Placements script
            placementScript.UpdateCheckpointDistance(this.gameObject, roadPath.Count);

            // Switches to next node on path
            if(Vector3.Distance(transform.position, temp) < 0.1f)
            {
                start = new Vector2Int(roadPath[0].X, roadPath[0].Y);
                roadPath.RemoveAt(0);
            }
            if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Mathf.RoundToInt(waypointList[0].transform.position.x), Mathf.RoundToInt(waypointList[0].transform.position.z))) < 0.1f)
            {
                if(fork)
                {
                    waypointList.RemoveAt(0);
                    waypointList.RemoveAt(0);
                    waypointList.RemoveAt(0);
                    fork = false;
                    skipFork = true;
                }

                if(waypointList[0].transform.position.y > 5)
                {
                    if(!fork && skipFork)
                    {
                        if(currentPlacement < savedPlacement)
                        {
                            object[] tempArr = new object[2];
                            tempArr[0] = otherForkWaypoint;
                            tempArr[1] = forksGoneThrough;
                            teammate1.SendMessage("RemoveWaypoint", tempArr);
                            teammate2.SendMessage("RemoveWaypoint", tempArr);
                        }
                        if(currentPlacement > savedPlacement)
                        {
                            object[] tempArr = new object[2];
                            tempArr[0] = currentForkWaypoint;
                            tempArr[1] = forksGoneThrough;
                            teammate1.SendMessage("RemoveWaypoint", tempArr);
                            teammate2.SendMessage("RemoveWaypoint", tempArr);
                        }
                    }
                    fork = true;
                    savedPlacement = currentPlacement;
                    forksGoneThrough += 1;
                    int randomPath = Random.Range(0, 2);
                    if(randomPath == 0)
                    {
                        waypointList[0] = waypointList[1];
                        currentForkWaypoint = waypointList[1];
                        otherForkWaypoint = waypointList[2];
                    }
                    else
                    {
                        waypointList[0] = waypointList[2];
                        currentForkWaypoint = waypointList[2];
                        otherForkWaypoint = waypointList[1];
                    }
                }
                else
                {
                    if(skipFork != true)
                    {
                        waypointList.RemoveAt(0);
                    }
                    if(!fork && skipFork)
                    {
                        skipFork = false;
                        if(currentPlacement < savedPlacement)
                        {
                            object[] tempArr = new object[2];
                            tempArr[0] = otherForkWaypoint;
                            tempArr[1] = forksGoneThrough;
                            teammate1.SendMessage("RemoveWaypoint", tempArr);
                            teammate2.SendMessage("RemoveWaypoint", tempArr);
                        }
                        if(currentPlacement > savedPlacement)
                        {
                            object[] tempArr = new object[2];
                            tempArr[0] = currentForkWaypoint;
                            tempArr[1] = forksGoneThrough;
                            teammate1.SendMessage("RemoveWaypoint", tempArr);
                            teammate2.SendMessage("RemoveWaypoint", tempArr);
                        }
                    }
                }
                start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
                if(waypointList.Count > 0)
                {
                    end = new Vector2Int(Mathf.RoundToInt(waypointList[0].transform.position.x), Mathf.RoundToInt(waypointList[0].transform.position.z));
                }
                roadPath.Clear();

                // Update amount of checkpoints in Placements script
                placementScript.UpdateCheckpointAmount(this.gameObject);
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

    // Removes waypoints for other teammates depending on if the racer changed placements
    public void RemoveWaypoint(object[] arr)
    {
        GameObject removeOtherWaypoint = (GameObject)arr[0];
        int forksPassed = (int)arr[1];
        if(!fork && forksGoneThrough < forksPassed)
        {
            for(int i = 0; i < waypointList.Count; i++)
            {
                if(removeOtherWaypoint == waypointList[i])
                {
                    waypointList.RemoveAt(i);
                    break;
                }
            }
            skipFork = true;
        }
    }

    // The racer's current position
    void RacerPosition()
    {
        for(int i = 0; i < placementScript.racerListOrdered.Count; i++)
        {
            if(this.gameObject == placementScript.racerListOrdered[i].Racer)
            {
                currentPlacement = i + 1;
            }
        }
    }

    #endregion

    #region UseItem

    int itemType = -1;
    void UseItem()
    {
        if(waypointList.Count > 0)
        {
            Movement();
        }
        
        itemType = Random.Range(0,3);
        switch(itemType)
		{
		case 0:
			SpeedUpItem();
			break;
        case 1:
			ProjectileItem();
			break;
        case 2:
			RadiusItem();
			break;
        }
        StartCoroutine(CheckWhosBehind());
        ChangeBehavior(RacerBehaviors.GoToGoal);
    }

    bool waitToUseItem = false;
    IEnumerator WaitToUseItem()
    {
        while(!waitToUseItem)
        {
            waitToUseItem = true;
            speed += 5f;
            yield return new WaitForSeconds(Random.Range(.5f, 2.5f));
        }
        waitToUseItem = false;
        ChangeBehavior(RacerBehaviors.UseItem);
        StopCoroutine(WaitToUseItem());
    }

    void SpeedUpItem()
    {
        StartCoroutine(SpeedUpItemTimer());
    }

    bool speedUpItemTimer = false;
    IEnumerator SpeedUpItemTimer()
    {
        while(!speedUpItemTimer)
        {
            speedUpItemTimer = true;
            speed += 5f;
            yield return new WaitForSeconds(.5f);
        }
        speed -= 5f;
        speedUpItemTimer = false;
        StopCoroutine(SpeedUpItemTimer());
    }

    Ray ray;
    public GameObject projectile;
    void ProjectileItem()
    {
        ray = new Ray(transform.position, transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject != teammate1 && hit.collider.gameObject != teammate2)
            {
                if(hit.collider.gameObject.tag == "Racer")
                {
                    Shoot();
                }
            }
        }
    }

    void Shoot()
    {
        Debug.Log("shoot");
        // Where the projectile will be created
        Vector3 shotSpawn = transform.position + (transform.forward * 2f);
        
        // Creates 1 projectile at shotSpawn
        GameObject projectileInstance = Instantiate(projectile, shotSpawn, transform.rotation) as GameObject;
        Rigidbody projectileRig = projectileInstance.GetComponent<Rigidbody>();
        projectileRig.velocity = transform.forward * 15f;
        Destroy (projectileInstance, 1.5f);
    }

    void RadiusItem()
    {
        if(Vector3.Distance(transform.position, teammate1.transform.position) > 1f &&
           Vector3.Distance(transform.position, teammate2.transform.position) > 1f)
        {
            teammatesInRadius = false;
        }
        else
        {
            teammatesInRadius = true;
        }
        StartCoroutine(RadiusItemCoroutine());
    }

    bool teammatesInRadius = true;
    IEnumerator RadiusItemCoroutine()
    {
        while(teammatesInRadius)
        {
            if(Vector3.Distance(transform.position, teammate1.transform.position) > 1f &&
               Vector3.Distance(transform.position, teammate2.transform.position) > 1f)
            {
                teammatesInRadius = false;
            }
            yield return new WaitForSeconds(1f);
        }

        for(int i = 0; i < placementScript.racerListOrdered.Count; i++)
        {
            if(Vector3.Distance(transform.position, placementScript.racerListOrdered[i].Racer.transform.position) < 1f && this.gameObject != placementScript.racerListOrdered[i].Racer)
            {
                placementScript.racerListOrdered[i].Racer.SendMessage("RadiusHit");
            }
        }
        StopCoroutine(RadiusItemCoroutine());
    }

    #endregion

    #region SpinOut

    float tempSpeed;
    void SpinOut()
    {
        speed = 0f;
        rotateRacer += Time.deltaTime * 360;
        transform.eulerAngles = new Vector3(0f, rotateRacer, 0f);
        
        if(rotateRacer > 360f)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            speed = tempSpeed;
            rotateRacer = 0f;
            m_ObjectCollider.isTrigger = false;
            StartCoroutine(CheckWhosBehind());
            // roadPath.Clear();
            ChangeBehavior(RacerBehaviors.GoToGoal);
        }
    }

    void RadiusHit()
    {
        m_ObjectCollider.isTrigger = true;
        StopCoroutine(CheckWhosBehind());
        ChangeBehavior(RacerBehaviors.SpinOut);
    }

    #endregion

    #region AllyEnemyAlly

    void Help()
    {
        if(Random.Range(0, 2) == 0 && !alreadyHelping)
        {
            alreadyHelping = true;
            StopCoroutine(CheckWhosBehind());
            StartCoroutine(SpeedUpTimer());
        }
    }

    IEnumerator SpeedUpTimer()
    {
        while(!speedUpTimer)
        {
            speedUpTimer = true;
            speed += 2f;
            yield return new WaitForSeconds(2f);
        }
        speed -= 2f;
        speedUpTimer = false;
        alreadyHelping = false;
        StopCoroutine(SpeedUpTimer());
    }

    void SlowDown()
    {
        speed -= 2f;
        StartCoroutine(SlowDownTimer());
    }

    IEnumerator SlowDownTimer()
    {
        while(!slowDownTimer)
        {
            slowDownTimer = true;
            yield return new WaitForSeconds(2f);
        }
        speed += 2;
        slowDownTimer = false;
        StopCoroutine(SlowDownTimer());
    }

    #endregion

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Enemy" || col.gameObject.tag == "OilSpill" || col.gameObject.tag == "Projectile")
        {
            m_ObjectCollider.isTrigger = true;
            StopCoroutine(CheckWhosBehind());
            ChangeBehavior(RacerBehaviors.SpinOut);
        }

        if(col.gameObject.tag == "Item")
        {
            StopCoroutine(CheckWhosBehind());
            StartCoroutine(WaitToUseItem());
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Racer" && alreadyHelping && col.gameObject != teammate1 && col.gameObject != teammate2)
        {
            col.gameObject.SendMessage("SlowDown");
        }
    }
}