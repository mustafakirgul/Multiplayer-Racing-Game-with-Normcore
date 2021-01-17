using System;
using UnityEngine;
public class WayPoint : MonoBehaviour
{
    public int index = -1;
    public bool defaultState;

    private void OnValidate()
    {
        //index = Convert.ToInt32(transform.name.Split("("[0])[1].Split(")"[0])[0]);
        GetComponent<MeshRenderer>().enabled = defaultState;
    }
}
