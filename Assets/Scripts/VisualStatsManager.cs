using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualStatsManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> StatsList = new List<GameObject>();

    [SerializeField]
    CarPhysicsParamsTemplate referenceForValues;

    private float maxHealth, maxWeight, maxacceleration, maxTopSpd, maxDefenseForce;
    // Start is called before the first frame update
    private void Awake()
    {
        UpdateMaximums();
    }
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
            StatsToDisplay.f_maxPlayerHealth / maxHealth;

            StatsList[1].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_rbWeight / maxWeight;

            StatsList[2].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_acceleration / maxacceleration;

            StatsList[3].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_topSpd / maxTopSpd;

            StatsList[4].transform.GetChild(0).GetComponent<Image>().fillAmount =
            StatsToDisplay.f_defenseForce / maxDefenseForce;
        }
    }

    private void OnValidate()
    {
        if(referenceForValues != null)
        {
            maxacceleration = referenceForValues.maxPlayerAcceleration;
            maxDefenseForce = referenceForValues.maxPlayerDefenseForce;
            maxHealth = referenceForValues.maxPlayerHealthRef;
            maxTopSpd = referenceForValues.maxPlayerTopSpd;
            maxWeight = referenceForValues.maxPlayerWeightRef;
        } 
    }

    public void UpdateMaximums()
    {
        if (referenceForValues != null)
        {
            maxacceleration = referenceForValues.maxPlayerAcceleration;
            maxDefenseForce = referenceForValues.maxPlayerDefenseForce;
            maxHealth = referenceForValues.maxPlayerHealthRef;
            maxTopSpd = referenceForValues.maxPlayerTopSpd;
            maxWeight = referenceForValues.maxPlayerWeightRef;
        }
    }
}
