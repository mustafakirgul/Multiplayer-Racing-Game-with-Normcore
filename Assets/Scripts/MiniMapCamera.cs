using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform _master;

    private void Update()
    {
        if (_master!=null)
        {
            transform.position = new Vector3(_master.position.x,transform.position.y,_master.position.z);
        }
    }
}
