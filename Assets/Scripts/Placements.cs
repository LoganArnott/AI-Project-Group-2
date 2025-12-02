using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Placements : MonoBehaviour
{
    public List<GameObject> racers = new List<GameObject>();
    public List<GameObject> checkpoints = new List<GameObject>();
    public List<RacerPlacement> racerList = new List<RacerPlacement>();
    public List<RacerPlacement> racerListOrdered = new List<RacerPlacement>();

    public TMP_Text placementsText;
    string placementsString = "";

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject racer in racers)
        {
            RacerPlacement temp = new RacerPlacement(racer, checkpoints);
            racerList.Add(temp);
        }
        placementsText.text = placementsString;
    }

    // Update is called once per frame
    void Update()
    {
        placementsString = "";
        racerListOrdered = new List<RacerPlacement>();
        SortPlacements();
        foreach(RacerPlacement racer in racerListOrdered)
        {
            placementsString += racer.Racer.name.ToString() + "\n";
        }
        placementsText.text = placementsString;
    }

    public void UpdateCheckpointDistance(GameObject racer, int checkpointDistance)
    {
        for(int i = 0; i < racerList.Count; i++)
        {
            if(racerList[i].Racer == racer)
            {
                racerList[i].SetCheckpointDistance(checkpointDistance);
            }
        }
    }

    public void UpdateCheckpointAmount(GameObject racer)
    {
        for(int i = 0; i < racerList.Count; i++)
        {
            if(racerList[i].Racer == racer)
            {
                racerList[i].PassedCheckpoint();
            }
        }
    }

    public void SortPlacements()
    {
        List<RacerPlacement> tempList1 = new List<RacerPlacement>();
        List<RacerPlacement> tempList2 = new List<RacerPlacement>();

        for(int i = 0; i < racerList.Count; i++)
        {
            tempList1.Add(racerList[i]);
        }

        tempList2 = BubbleSortCheckpointAmount(tempList1);

        BubbleSortCheckpointDistance(tempList2);
    }

    public List<RacerPlacement> BubbleSortCheckpointAmount(List<RacerPlacement> sortList)
    {
        RacerPlacement temp;
        bool swapped;
        for(int i = 0; i < sortList.Count - 1; i++)
        {
            swapped = false;
            for(int j = 0; j < sortList.Count - i - 1; j++)
            {
                if(sortList[j].checkpointAmount > sortList[j + 1].checkpointAmount)
                {
                    temp = sortList[j];
                    sortList[j] = sortList[j + 1];
                    sortList[j + 1] = temp;
                    swapped = true;
                }
            }

            if(swapped == false)
            {
                break;
            }
        }
        
        return sortList;
    }

    public void BubbleSortCheckpointDistance(List<RacerPlacement> sortList)
    {
        RacerPlacement temp;
        bool swapped;
        for(int i = 0; i < sortList.Count - 1; i++)
        {
            swapped = false;
            for(int j = 0; j < sortList.Count - i - 1; j++)
            {
                if(sortList[j].checkpointDistance > sortList[j + 1].checkpointDistance && sortList[j].checkpointAmount == sortList[j + 1].checkpointAmount)
                {
                    temp = sortList[j];
                    sortList[j] = sortList[j + 1];
                    sortList[j + 1] = temp;
                    swapped = true;
                }
            }

            if(swapped == false)
            {
                break;
            }
        }
        
        for(int i = 0; i < racerList.Count; i++)
        {
            racerListOrdered.Add(sortList[i]);
        }
    }
}
