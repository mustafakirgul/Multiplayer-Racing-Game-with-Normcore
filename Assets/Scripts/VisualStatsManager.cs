﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualStatsManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> StatsList = new List<GameObject>();
    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            StatsList.Add(this.transform.GetChild(i).gameObject);
        }
    }
    public void SetVisualStats(CarPhysicsParamsSObj StatsToDisplay)
    {
        if(StatsList.Count != 0)
        {
            StatsList[0].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_maxPlayerHealth / 100f;

            StatsList[1].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_rbWeight / 100;

            StatsList[2].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_acceleration / 2f;

            StatsList[3].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_topSpd / 2f;

            StatsList[4].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_defenseForce / 1f;
        }
    }
}
