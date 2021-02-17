using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public float smoothTime;
    public float lookAtLERPSpeed;
    public float lookAtDistance;
    public Rigidbody target;
    public Transform cameraPlaceTarget;
    private bool isInitialized;
    Vector3 _temp, velocity;

    public void InitCamera(Rigidbody _target)
    {
        if (!isInitialized)
        {
            target = _target;
            cameraPlaceTarget = target.transform.GetChild(0).transform;
            isInitialized = true;
        }
    }

    public void ResetCam()
    {
        isInitialized = false;
    }

    private void LateUpdate()
    {
        if (isInitialized)
        {
            _temp = target.transform.TransformPoint(cameraPlaceTarget.localPosition);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                _temp,
                ref velocity,
                smoothTime * Time.deltaTime
            );

            Debug.DrawLine(transform.position, _temp, Color.blue);
            Debug.DrawLine(target.transform.position, _temp, Color.blue);

            _temp = target.transform.position + (target.transform.forward * lookAtDistance);

            Debug.DrawLine(transform.position, _temp, Color.green);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(
                    (_temp - transform.position).normalized
                ),
                lookAtLERPSpeed * Time.deltaTime
            );
        }
    }
}