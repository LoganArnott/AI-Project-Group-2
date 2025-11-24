using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        Debug.Log("hello");
    }
}
