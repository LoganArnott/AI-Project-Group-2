using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToInt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
