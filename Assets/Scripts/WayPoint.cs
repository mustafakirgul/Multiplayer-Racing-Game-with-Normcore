using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public int index = -1;
    public bool defaultState;

    private void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            string _temp = transform.name.Split("("[0])[1];
            _temp = _temp.Remove(_temp.Length - 1, 1);
            index = Convert.ToInt32(_temp);
            GetComponent<MeshRenderer>().enabled = defaultState;
        }
    }
}