using System;
using UnityEngine;

public class ChaseCam : MonoBehaviour
{
    // The target we are following
    public Transform target, lookAtTarget, parent;
    private bool isInitialized = false;

    public float lookAtSpeed;
    public float positionSpeed;
    private Vector3 lookPosition, targetPosition, lookTargetPosition;
    public float minY, lookY;
    public LayerMask mask;
    public MeshRenderer meshSphere;

    private void OnDrawGizmos()
    {
        if (target == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAtTarget.position, 1f);
        Gizmos.DrawWireCube(lookPosition, Vector3.one);
    }

    public void OpenCameraCover()
    {
        meshSphere.enabled = false;
    }

    public void CloseCameraCover()
    {
        meshSphere.enabled = true;
    }

    private void Start()
    {
        CloseCameraCover();
    }

    public void InitCamera(Transform _target, Transform _lookAtTarget)
    {
        if (!isInitialized)
        {
            target = _target;
            lookAtTarget = _lookAtTarget;
            parent = target.parent;
            isInitialized = true;
        }
    }

    public void ResetCam()
    {
        isInitialized = false;
    }

    private void LateUpdate()
    {
        if (target == null) isInitialized = false;
        if (isInitialized)
        {
            RaycastHit hit;
            var bottomLimit = 0f;
            Physics.Raycast(target.position, Vector3.down, out hit, Mathf.Infinity, mask);
            Debug.DrawLine(target.position, hit.point, Color.yellow);
            if (hit.distance > target.localPosition.y) //Safe Guard
                bottomLimit = parent.position.y;
            else
                bottomLimit = parent.position.y + minY;

            targetPosition = new Vector3(target.position.x,
                Mathf.Clamp(hit.point.y + minY, bottomLimit, Mathf.Infinity), target.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSpeed);
            if (hit.distance > lookAtTarget.localPosition.y) //Safe Guard
                bottomLimit = parent.position.y;
            else
                bottomLimit = parent.position.y + lookY;

            Physics.Raycast(lookAtTarget.position, Vector3.down, out hit, Mathf.Infinity, mask);
            Debug.DrawLine(lookAtTarget.position, hit.point, Color.yellow);
            lookTargetPosition = new Vector3(lookAtTarget.position.x,
                Mathf.Clamp(hit.point.y + lookY, bottomLimit, Mathf.Infinity), lookAtTarget.position.z);
            lookPosition = Vector3.Lerp(lookPosition, lookTargetPosition, Time.deltaTime * lookAtSpeed);
            transform.LookAt(lookPosition);
        }
    }
}