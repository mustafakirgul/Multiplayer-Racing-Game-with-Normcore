using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera _camera => Camera.main;

    private void Update()
    {
        if (_camera == null) return;
        transform.LookAt(_camera.transform);
    }
}