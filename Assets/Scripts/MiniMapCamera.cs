using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform _master;
    public float desiredHeight;

    private void Update()
    {
        if (_master != null)
        {
            transform.position =
                new Vector3(_master.position.x, _master.position.y + desiredHeight, _master.position.z);
        }
    }
}