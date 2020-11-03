using UnityEngine;
using Normal.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class WC_Car_Controller : MonoBehaviour
{
    public float torque;
    public float currentTorque;
    public bool limitTopSpeed;
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
    public TextMeshProUGUI speedDisplay, IDDisplay;
    private ParticleSystem.EmissionModule dustEmission, pebbleEmission;
    public ParticleSystem dustParticles, pebbles;
    [HideInInspector]
    public float actualMaxSpeed;
    [HideInInspector]
    public float totalRPM, RPM;
    public float maxTheoreticalRPM;
    int wheelCount;
    WheelHit _hit;
    public Terrain currentTerrainType = Terrain.nothing;
    private TerrainType terrainType;
    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;
    [HideInInspector]
    public Realtime _realtime;
    public Transform cameraPlace, lookAtTarget;
    Camera_Controller chaseCam;
    bool isNetworkInstance;
    public float dashForce;
    public bool offlineTest = true;
    public int numberOfTiresTouchingGround = 0;

    public Player _player;
    public string _currentName;

    [Space]
    public bool enableBoost = true;
    public float boostCooldownTime = 5f;
    public Image boostRadialLoader;
    public bool boosterReady;
    private float boosterCounter;
    private WaitForEndOfFrame waitFrame;
    private Coroutine boostCounter;

    private UIManager uIManager;
    private void Awake()
    {
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        if (!offlineTest)
        {
            _realtimeView.enabled = true;
            _realtimeTransform.enabled = true;
            _player = GetComponent<Player>();
        }
    }
    private void Start()
    {
        dustEmission = dustParticles.emission;
        pebbleEmission = pebbles.emission;
        if (_realtimeView.isOwnedLocallySelf)
        {
            isNetworkInstance = false;
            carBody.Sleep();
            uIManager = GameObject.FindObjectOfType<UIManager>();
            uIManager.EnableUI();
            speedDisplay = uIManager.speedometer;
            IDDisplay.gameObject.SetActive(false);
            IDDisplay = uIManager.playerName;
            if (_currentName != _player.playerName)
            {
                _currentName = _player.playerName;
                IDDisplay.SetText(_currentName);
            }
            boostRadialLoader = uIManager.boostRadialLoader;
            waitFrame = new WaitForEndOfFrame();
            boostCounter = StartCoroutine(BoostCounter());
            InitCam();
            return;
        }
        else
        {
            if (offlineTest)
                InitCam();
            isNetworkInstance = true;
        }

        wheelCount = wheels.Count;
        for (int i = 0; i < wheelCount; i++)
        {
            wheels[i].collider.wheelDampingRate = wheels[i].dampingRate;
            if (wheels[i].trail != null)
            {
                wheels[i].trail.emitting = false;
            }
        }

        actualMaxSpeed = maxSpeed / speedDisplayMultiplier;

    }
    public IEnumerator BoostCounter()
    {
        while (enableBoost)
        {
            if (!boosterReady)
            {
                if (boosterCounter < boostCooldownTime)
                {
                    boostRadialLoader.enabled = true;
                    boosterCounter += Time.deltaTime;
                    boostRadialLoader.fillAmount = boosterCounter / boostCooldownTime;
                }
                else
                {
                    boostRadialLoader.fillAmount = 1f;
                    boosterReady = true;
                    boosterCounter = 0f;
                }
            }
            else
            {
                boostRadialLoader.enabled = Time.realtimeSinceStartup % 1f > .05f;
            }
            yield return waitFrame;
        }
    }

    private void InitCam()
    {
        chaseCam = GameObject.FindObjectOfType<Camera_Controller>();
        chaseCam.InitCamera(gameObject, cameraPlace, lookAtTarget);
    }

    private void Update()
    {
        if (isNetworkInstance)
            return;
        if (!offlineTest)
        {
            for (int i = 0; i < wheelCount; i++)
            {
                if (wheels[i].model != null)
                {
                    if (wheels[i].model.GetComponent<RealtimeView>() != null)
                    {
                        wheels[i].model.GetComponent<RealtimeView>().enabled = true;
                        if (wheels[i].model.GetComponent<RealtimeTransform>() != null)
                        {
                            wheels[i].model.GetComponent<RealtimeTransform>().enabled = true;
                        }
                    }
                }
            }

            _realtimeTransform.RequestOwnership();
            for (int i = 0; i < wheelCount; i++)
            {
                wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
                if (wheels[i].model.GetComponent<RealtimeView>().isOwnedLocallySelf)
                    wheels[i].model.GetComponent<RealtimeTransform>().RequestOwnership();
            }
        }
        else
        {

        }
        ListenForInput();
        velocity = Mathf.Abs(transform.InverseTransformVector(carBody.velocity).z);
        sidewaysVelocity = Mathf.Abs(transform.InverseTransformVector(carBody.velocity).x);

        ////Downwards force test
        //carBody.AddRelativeForce(
        //    -transform.up * (
        //    (100 * velocity) +
        //    (100 * verticalInput) +
        //    (100 * sidewaysVelocity)),
        //    ForceMode.Force
        //    );

        currentTorque = verticalInput * torque;
        if (velocity < .333f && verticalInput == 0f)
        {
            isBraking = true;
        }
        if (currentTorque > 0)
        {
            dustEmission.rateOverTime =
                Mathf.Clamp((currentTorque / torque) *
                dustFactor,
                0,
                dustLimit);
            pebbleEmission.rateOverTime =
                Mathf.Clamp((currentTorque / torque) *
                pebbleFactor,
                0,
                pebbleLimit);
        }
        else
        {
            if (isBraking)
                dustEmission.rateOverTime = velocity * brakeDustFactor;

            if (sidewaysVelocity > (velocity / 2f))
            {
                dustEmission.rateOverTime = (velocity * brakeDustFactor) * 2f;
            }
            else dustEmission.rateOverTime = .1f;

            pebbleEmission.rateOverTime = .1f;
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
        RunWheels();
        if (limitTopSpeed)
            carBody.velocity = Vector3.ClampMagnitude(carBody.velocity, actualMaxSpeed);
    }

    private void RunWheels()
    {
        numberOfTiresTouchingGround = 0;
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

            if (wheel.model != null)
            {
                Quaternion _rotation;
                Vector3 _position;

                wheel.collider.GetWorldPose(out _position, out _rotation);

                wheel.model.transform.position = _position;
                wheel.model.transform.rotation = _rotation;
            }

            if (wheel.trail != null)
            {
                if (drawSkidmark)
                {
                    _hit = new WheelHit();
                    wheel.collider.GetGroundHit(out _hit);
                    if (_hit.collider != null)
                    {
                        terrainType = _hit.collider.gameObject.GetComponent<TerrainType>();
                        if (terrainType != null)
                        {
                            numberOfTiresTouchingGround++;
                            currentTerrainType = terrainType.type;
                        }

                        else
                            currentTerrainType = Terrain.nothing;

                        wheel.trail.transform.position = _hit.point + skidmarkOffset;
                    }
                    else
                        currentTerrainType = Terrain.nothing;

                    wheel.trail.emitting = currentTerrainType == Terrain.sand;
                }
                else if (wheel.trail.gameObject.activeSelf)
                {
                    wheel.trail.gameObject.SetActive(false);
                }
            }
        }
        speedDisplay.text = Mathf.RoundToInt((velocity * speedDisplayMultiplier)).ToString();
    }

    private void ListenForInput()
    {
        verticalInput = Mathf.Lerp(verticalInput, Input.GetAxisRaw("Vertical"), .1f);
        if (clutchTimer < clutchTime && verticalInput > 0f)
        {
            verticalInput *= Mathf.Clamp(clutchCurve.Evaluate(clutchTimer / clutchTime), -1, 1);
            clutchTimer += Time.deltaTime;
        }
        else if (verticalInput <= .01f)
        {
            clutchTimer = 0f;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.R))//reset
        {
            if (Physics.Raycast(transform.position, transform.up, upSideDownCheckRange))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + resetHeight, transform.position.z);
                Vector3 _rotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
                carBody.velocity = Vector3.zero;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))//lights
        {
            lights = !lights;
            RHL.enabled = lights;
            LHL.enabled = lights;
        }
        isBraking = Input.GetKey(KeyCode.Space);//handbrake;
        if (Input.GetKeyDown(KeyCode.Q) && numberOfTiresTouchingGround > 0)//boost/dash
        {
            if (boosterReady)
            {
                boosterReady = false;
                carBody.AddForce(transform.forward * dashForce, ForceMode.VelocityChange);
            }
        }
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
