using System;
using UnityEngine;

public class CountdownLights : MonoBehaviour
{
    public GameObject[] lights;

    public void TurnOntheLight(int index)
    {
        lights[index].SetActive(true);
    }
}