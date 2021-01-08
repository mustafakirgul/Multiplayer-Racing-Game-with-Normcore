using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCam : MonoBehaviour
{
    public Transform target;
    Vector3 lookAtTarget, positionTarget;
    public Vector3 placeOffset, lookAtOffset;
    public float LERPSpeed;
    public bool drawDebugLines;
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
            lookAtTarget = target.TransformPoint(lookAtOffset);
            positionTarget = target.TransformPoint(placeOffset);
            if (drawDebugLines)
            {
                Debug.DrawLine(target.position, lookAtTarget, Color.red);
                Debug.DrawLine(target.position, positionTarget, Color.yellow);
                Debug.DrawLine(transform.position, lookAtTarget, Color.green);
                Debug.DrawLine(transform.position, positionTarget, Color.blue);
            }
            transform.LookAt(lookAtTarget);
            transform.position = Vector3.Lerp(
                transform.position,
                positionTarget,
                1 - Mathf.Exp(-Time.deltaTime * LERPSpeed));
        }
        else if (target != null)
        {
            InitCamera(target);
        }
    }
}
