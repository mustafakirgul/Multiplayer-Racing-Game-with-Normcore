using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCam : MonoBehaviour
{
    public Transform target;
    Vector3 lookAtTarget, positionTarget;
    public Vector3 placeOffset, lookAtOffset;
    public float LERPSpeed;
    private bool isInitialized;

    private void Start()
    {
        if (target != null)
        {
            InitCamera(target);
        }
    }

    public void InitCamera(Transform _target)
    {
        if (!isInitialized)
        {
            target = _target;
            isInitialized = true;
        }
    }
    private void LateUpdate()
    {
        if (isInitialized)
        {
            lookAtTarget = target.position + lookAtOffset;
            positionTarget = target.position + placeOffset;
            transform.LookAt(lookAtTarget);
            transform.position = Vector3.Lerp(
                transform.position,
                positionTarget,
                Time.deltaTime * LERPSpeed);
        }
        else if (target != null)
        {
            InitCamera(target);
        }
    }
}
