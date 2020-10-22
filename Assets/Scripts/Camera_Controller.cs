using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public Transform place;
    public Transform lookAtTarget;
    public float followSpeed;
    public GameObject velocityReference;
    public float slowFocalLength, fastFocalLength;
    private float zoomRange;
    private float currentZoomLevel;
    [SerializeField]
    private float focalLength;
    public bool overrideCameraY;
    public float overrideCameraYValue;
    private Camera _camera;

    public WC_Car_Controller wcController;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (wcController != null)
        {
            zoomRange = slowFocalLength - fastFocalLength;
            currentZoomLevel = wcController.velocity / wcController.actualMaxSpeed;
            focalLength = slowFocalLength - (currentZoomLevel * zoomRange);
        }
    }

    private void LateUpdate()
    {
        if (wcController != null)
        {
            transform.LookAt(lookAtTarget);

            if (overrideCameraY)
            {
                transform.position = Vector3.Lerp(transform.position
                , new Vector3(place.position.x, overrideCameraYValue, place.position.z)
                , Time.deltaTime * followSpeed);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position
                , place.position
                , Time.deltaTime * followSpeed);
            }
            _camera.focalLength = focalLength;
        }
    }
}
