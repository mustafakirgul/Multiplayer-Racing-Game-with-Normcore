using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCam : MonoBehaviour
{
    // The target we are following
    public Transform target;
    private bool isInitialized = false;

    // The distance in the x-z plane to the target
    public float distance = 10.0f;

    // the height we want the camera to be above the target
    public float height = 5.0f;
    public float rotationDamping;
    public float heightDamping;

    public bool bToggleRearView = false;
    public float desiredHeight;
    private float hitDistance, calculatedY, targetY;
    private Vector3 lookPosition;
    public float lookPositionLerpSpeed;
    public float minY = 4f;

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

    public void ToggleRearView(Transform _target)
    {
        target = _target;
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
            float wantedRotationAngle = target.eulerAngles.y;
            //float wantedHeight = target.position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            //float currentHeight = transform.position.y;

            if (!bToggleRearView)
            {
                // Damp the rotation around the y-axis
                currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle,
                    rotationDamping * Time.deltaTime);

                // Damp the height
                //currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            }
            else
            {
                currentRotationAngle = wantedRotationAngle;
                //currentHeight = wantedHeight;
            }

            // Convert the angle into a rotation
            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            var _target = target.position - currentRotation * Vector3.forward * distance;

            // Set the height of the camera
            if (Physics.Raycast(_target, Vector3.down, out RaycastHit hit0, minY))
                _target = new Vector3(_target.x, hit0.point.y + minY, _target.z);
            else if (Physics.Raycast(_target, Vector3.up, out RaycastHit hit1, minY * 3f))
                _target = new Vector3(_target.x, hit1.point.y + minY, _target.z);

            transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime * 13.2f);


            //raycast to see the height of target from ground
            Physics.Raycast(target.position, Vector3.down, out RaycastHit hit2, Mathf.Infinity);
            hitDistance = hit2.distance;
            targetY = Mathf.Clamp(hit2.point.y + desiredHeight, target.parent.position.y,
                target.position.y + desiredHeight);
            calculatedY = Mathf.Lerp(calculatedY, targetY, Time.deltaTime * lookPositionLerpSpeed);
            lookPosition = new Vector3(target.position.x, calculatedY, target.position.z);
            // Always look at the target
            transform.LookAt(lookPosition);
        }
        else if (target != null)
        {
            InitCamera(target);
        }
    }
}