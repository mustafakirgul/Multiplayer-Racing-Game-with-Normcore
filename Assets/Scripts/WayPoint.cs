using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public int index = -1;

    private void Awake()
    {
        if (index < 0)
            index = Convert.ToInt32(transform.name.Split(" "[0])[1]);
        GetComponent<MeshRenderer>().enabled = false;
    }
}
