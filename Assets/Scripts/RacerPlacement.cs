using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerPlacement
{
    public GameObject Racer;
    public List<GameObject> Checkpoints = new List<GameObject>();
    public int checkpointDistance = 100000;
    public int checkpointAmount = 100000;

    public RacerPlacement(GameObject racer, List<GameObject> checkpoints)
    {
        Racer = racer;
        for(int i = 0; i < checkpoints.Count; i++)
        {
            Checkpoints.Add(checkpoints[i]);
        }
    }

    public void PassedCheckpoint()
    {
        if(Checkpoints.Count > 0)
        {
            Checkpoints.RemoveAt(0);
            SetCheckpointAmount();
        }
    }

    public void SetCheckpointDistance(int tiles)
    {
        checkpointDistance = tiles;
    }

    public void SetCheckpointAmount()
    {
        checkpointAmount = Checkpoints.Count;
    }
}
