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
            float wantedHeight = target.position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            if (!bToggleRearView)
            {
                // Damp the rotation around the y-axis
                currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle,
                    rotationDamping * Time.deltaTime);

                // Damp the height
                currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);


                // Convert the angle into a rotation
                Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

                // Set the position of the camera on the x-z plane to:
                // distance meters behind the target
                transform.position = target.position;
                transform.position -= currentRotation * Vector3.forward * distance;
            }
            else
            {
                currentRotationAngle = wantedRotationAngle;
                currentHeight = wantedHeight;
                transform.position = target.position;
                Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
                transform.position -= currentRotation * Vector3.forward * distance;
            }

            // Set the height of the camera
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(target);
        }
        else if (target != null)
        {
            InitCamera(target);
        }
    }
}