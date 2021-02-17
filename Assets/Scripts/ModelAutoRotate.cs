using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAutoRotate : MonoBehaviour
{
    bool isRotating;
    public float autoRotatioSpd;

    private void OnEnable()
    {
        isRotating = true;
    }

    private void Update()
    {
        if (isRotating)
            this.transform.Rotate(Vector3.up, Time.deltaTime * autoRotatioSpd, Space.Self);
    }

    private void OnDisable()
    {
        isRotating = false;
    }
}