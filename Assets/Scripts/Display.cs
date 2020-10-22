using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour
{
    private Transform mainCam;

    private void Start()
    {
        mainCam = Camera.main.transform;
    }
    void Update()
    {
        transform.LookAt(mainCam);
    }
}
