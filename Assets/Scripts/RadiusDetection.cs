using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiusDetection : MonoBehaviour
{
    public List<GameObject> racersInRadius = new List<GameObject>();
    Vector3 destination;
	float distance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Racer")
        {
            racersInRadius.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "Racer")
        {
            for(int i = 0; i < racersInRadius.Count; i++)
            {
                if(col.gameObject == racersInRadius[i])
                {
                    racersInRadius.RemoveAt(i);
                }
            }
        }
    }
}
