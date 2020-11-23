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
    public float overrideCameraYOffset;
    private Camera _camera;
    private bool isInitialized;
    private Transform YReference;
    private WC_Car_Controller wcController;
    public bool fixedFocalLength;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    public void InitCamera(GameObject _target, Transform _place, Transform _lookTarget)
    {
        if (!isInitialized)
        {
            wcController = _target.GetComponent<WC_Car_Controller>();
            velocityReference = _target;
            YReference = velocityReference.transform;
            place = _place;
            lookAtTarget = _lookTarget;
            place.gameObject.SetActive(false);
            lookAtTarget.gameObject.SetActive(false);
            isInitialized = true;
        }
    }

    public void ResetCam()
    {
        isInitialized = false;
    }

    private void Update()
    {
        if (!fixedFocalLength)
        {
            if (isInitialized)
            {
                if (wcController != null)
                {
                    zoomRange = slowFocalLength - fastFocalLength;
                    currentZoomLevel = wcController.velocity / wcController.actualMaxSpeed;
                    focalLength = slowFocalLength - (currentZoomLevel * zoomRange);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (isInitialized)
        {
            if (wcController != null)
            {
                transform.LookAt(lookAtTarget);

                if (overrideCameraY)
                {
                    transform.position = Vector3.Lerp(transform.position
                    , new Vector3(place.position.x,
                    YReference.position.y + overrideCameraYOffset,
                    place.position.z)
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
}
