using UnityEngine;
using Normal.Realtime;
using TMPro;
using System.Collections.Generic;
using TMPro.Examples;

public class WC_Car_Controller : MonoBehaviour
{
    public float torque;
    public float currentTorque;
    [SerializeField]
    private float maxSpeed;
    public AnimationCurve clutchCurve;
    public float clutchTime = 0.5f;
    public float clutchTimer = 0f;
    [Range(0, 1)]
    public float steeringSpeed;
    public float maxSteering;
    public float brakePower;
    public float resetHeight;
    public float upSideDownCheckRange;
    public float verticalInput, horizontalInput;
    public float speedDisplayMultiplier = 5f;
    public Light RHL, LHL;
    private bool lights;
    public Rigidbody carBody;
    public List<Wheel> wheels;
    public bool isBraking;
    public float velocity, sidewaysVelocity;
    public bool drawSkidmark = false;
    public Vector3 skidmarkOffset;
    public int brakeDustFactor, brakeDustLimit, dustFactor, dustLimit, pebbleFactor, pebbleLimit;
    public TextMeshProUGUI display;
    private ParticleSystem.EmissionModule dustEmission, pebbleEmission;
    public ParticleSystem dustParticles, pebbles;
    [HideInInspector]
    public float actualMaxSpeed;
    //[HideInInspector]
    public float totalRPM, RPM, maxTheoreticalRPM;
    int wheelCount;
    WheelHit _hit;
    public Terrain currentTerrainType = Terrain.nothing;
    private TerrainType terrainType;
    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;
    public Transform cameraPlace, lookAtTarget;
    Camera_Controller chaseCam;
    bool isNetworkInstance;

    private void Awake()
    {
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();

    }
    private void Start()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (!_realtimeView.isOwnedLocallySelf)
        {
            gameObject.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            isNetworkInstance = true;
            carBody.Sleep();
            return;
        }
        else
        {
            chaseCam = GameObject.FindObjectOfType<Camera_Controller>();
            chaseCam.place = cameraPlace;
            chaseCam.lookAtTarget = lookAtTarget;
            chaseCam.velocityReference = gameObject;
            chaseCam.wcController = this;
        }
        wheelCount = wheels.Count;
        for (int i = 0; i < wheelCount; i++)
        {
            wheels[i].collider.wheelDampingRate = wheels[i].dampingRate;
        }
        dustEmission = dustParticles.emission;
        pebbleEmission = pebbles.emission;
        display = GetComponentInChildren<TextMeshProUGUI>();
        actualMaxSpeed = maxSpeed / speedDisplayMultiplier;

    }
    private void Update()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (isNetworkInstance)
            return;
        // Make sure we own the transform so that RealtimeTransform knows to use this client's transform to synchronize remote clients.
        _realtimeTransform.RequestOwnership();
        ListenForInput();
        velocity = Mathf.Abs(transform.InverseTransformVector(carBody.velocity).z);
        sidewaysVelocity = Mathf.Abs(carBody.velocity.z);
        currentTorque = verticalInput * torque;
        if (currentTorque > 0)
        {
            dustEmission.rateOverTime =
                Mathf.Clamp((1f - (currentTorque / torque)) *
                dustFactor,
                0,
                dustLimit);
            pebbleEmission.rateOverTime =
                Mathf.Clamp((1f - (currentTorque / torque)) *
                pebbleFactor,
                0,
                pebbleLimit);
        }
        else
        {
            if (isBraking)
                dustEmission.rateOverTime = velocity * brakeDustFactor;
            else
            {
                if (sidewaysVelocity > (velocity / 2f))
                {
                    dustEmission.rateOverTime = (velocity * brakeDustFactor) * 2f;
                }
                else dustEmission.rateOverTime = 0f;
                pebbleEmission.rateOverTime = 0f;
            }
        }

        totalRPM = 0f;
        for (int i = 0; i < wheelCount; i++)
        {
            totalRPM += wheels[i].collider.rpm;
        }
        RPM = totalRPM / wheelCount;
    }

    private void LateUpdate()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (isNetworkInstance)
            return;

        display.text = Mathf.RoundToInt((velocity * speedDisplayMultiplier)).ToString();
        RunWheels();
        carBody.velocity = Vector3.ClampMagnitude(carBody.velocity, actualMaxSpeed);
    }

    private void RunWheels()
    {
        foreach (Wheel wheel in wheels)
        {
            if (isBraking)
            {
                wheel.collider.motorTorque = 0f;
                wheel.collider.brakeTorque = brakePower;
            }
            else
            {
                wheel.collider.brakeTorque = 0f;
                if (wheel.isPowered)
                {
                    wheel.collider.motorTorque = currentTorque / wheels.Count;
                }
            }

            if (wheel.isSteeringWheel)
            {
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, horizontalInput * maxSteering, steeringSpeed);
            }

            Quaternion _rotation;
            Vector3 _position;

            wheel.collider.GetWorldPose(out _position, out _rotation);

            wheel.model.transform.position = _position;
            wheel.model.transform.rotation = _rotation;

            if (drawSkidmark)
            {
                wheel.collider.GetGroundHit(out _hit);
                if (_hit.collider != null)
                {
                    terrainType = _hit.collider.gameObject.GetComponent<TerrainType>();
                    if (terrainType != null)
                        currentTerrainType = terrainType.type;
                    else
                        currentTerrainType = Terrain.nothing;
                }
                wheel.trail.emitting = currentTerrainType == Terrain.sand;
                wheel.trail.transform.position = _hit.point + skidmarkOffset;
            }
            else if (wheel.trail.gameObject.activeSelf)
            {
                wheel.trail.gameObject.SetActive(false);
            }
        }
    }

    private void ListenForInput()
    {
        verticalInput = Mathf.Lerp(verticalInput, Input.GetAxisRaw("Vertical"), .1f);
        if (clutchTimer < clutchTime && verticalInput > 0f)
        {
            verticalInput *= Mathf.Clamp(clutchCurve.Evaluate(clutchTimer / clutchTime), -1, 1);
            clutchTimer += Time.deltaTime;
        }
        else if (verticalInput <= 0f)
        {
            clutchTimer = 0f;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown("r"))//reset
        {
            if (Physics.Raycast(transform.position, transform.up, upSideDownCheckRange))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + resetHeight, transform.position.z);
                Vector3 _rotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
                carBody.velocity = Vector3.zero;
            }
        }
        if (Input.GetKeyDown("e"))//lights
        {
            lights = !lights;
            RHL.enabled = lights;
            LHL.enabled = lights;
        }
        isBraking = Input.GetKey(KeyCode.Space);//handbrake;
    }
    [System.Serializable]
    public struct Wheel
    {
        public GameObject model;
        public WheelCollider collider;
        public bool isSteeringWheel;
        public bool isPowered;
        public TrailRenderer trail;
        public float dampingRate;
    }
}
