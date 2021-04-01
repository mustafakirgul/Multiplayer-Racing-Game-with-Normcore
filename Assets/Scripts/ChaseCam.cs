using UnityEngine;

public class ChaseCam : MonoBehaviour
{
    // The target we are following
    public Transform target;
    public Transform lookTarget;

    private bool isInitialized = false;

    public float lookAtSpeed;
    public float positionSpeed;
    private Vector3 lookPosition;
    public Vector3 _target;

    private void OnDrawGizmos()
    {
        if (target == null || _target == Vector3.zero) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 1f);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    public void InitCamera(Transform _target, Transform _lookTarget)
    {
        if (!isInitialized)
        {
            target = _target;
            lookTarget = _lookTarget;
            isInitialized = true;
        }
    }

    public void ResetCam()
    {
        isInitialized = false;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            isInitialized = false;
        }

        if (isInitialized)
        {
            _target = target.position;
            transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime * positionSpeed);
            Debug.DrawLine(transform.position, _target, Color.green);
            lookPosition = Vector3.Lerp(lookPosition,lookTarget.position, Time.deltaTime * lookAtSpeed);
            transform.LookAt(lookPosition);
        }
    }
}