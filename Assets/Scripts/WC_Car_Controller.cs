using UnityEngine;
using Normal.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class WC_Car_Controller : MonoBehaviour
{
    //Car tunables
    public float torque;
    public float currentTorque;

    [SerializeField]
    private float torqueDistributionValue;

    public bool limitTopSpeed;
    [SerializeField]
    private float maxSpeed = 200f;
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
    public bool turnAid;
    public float turnAidLevel;

    //UI display
    public TextMeshProUGUI speedDisplay, IDDisplay;
    private UIManager uIManager;

    //Trail and particles systems
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

    //Camera tunables
    public Transform cameraPlace, lookAtTarget;
    Camera_Controller chaseCam;
    bool isNetworkInstance;
    public float dashForce;
    public bool offlineTest = true;
    public int numberOfTiresTouchingGround = 0;

    public Player _player;
    public string _currentName;

    //Boost UI
    [Space]
    public bool enableBoost = true;
    public float boostCooldownTime = 5f;
    public Image boostRadialLoader;
    public bool boosterReady;
    private float boosterCounter;
    private WaitForEndOfFrame waitFrame, waitFrame2;
    private WaitForSeconds wait, muzzleWait;

    //Health UI
    public Image healthRadialLoader;
    public float m_fplayerLastHealth;
    public GameObject DeathExplosion;
    bool isPlayerAlive;

    public Material[] CarStates;

    GameObject _bulletBuffer;


    //Weapons systems
    [SerializeField]
    private GameObject WeaponProjectile;

    [SerializeField]
    private List<GameObject> WeaponProjectiles;

    public Transform _barrelTip;
    public float fireRate;//number of bullets fired per second //Weapon Change should affect this variable
    public bool readyToFire = false;
    public GameObject muzzleFlash;
    float fireTimer;

    public SpriteRenderer _miniMapRenderer;

    public bool experimentalCar;
    [HideInInspector]
    public int _bombs;
    [HideInInspector]
    public int _resets;

    public float explosionForce = 2000000f;
    bool inReverse;
    float trueVelocity;
    //[HideInInspector]
    public int ownerID;

    public float centerOfMassY;
    Vector3 initialCenterOfMass;
    private void Awake()
    {
        initialCenterOfMass = carBody.centerOfMass;
        carBody.centerOfMass = new Vector3(carBody.centerOfMass.x, centerOfMassY, carBody.centerOfMass.z);

        _realtime = FindObjectOfType<Realtime>();
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        ownerID = _realtime.room.clientID;
        if (!offlineTest)
        {
            _realtimeView.enabled = true;
            _realtimeTransform.enabled = true;
            _player = GetComponent<Player>();
            waitFrame = new WaitForEndOfFrame();
            waitFrame2 = new WaitForEndOfFrame();
            fireTimer = 1f / fireRate;
            wait = new WaitForSeconds(fireTimer);
            muzzleWait = new WaitForSeconds(.2f);
        }
    }
    private void Start()
    {
        if (!experimentalCar)
        {
            dustEmission = dustParticles.emission;
            pebbleEmission = pebbles.emission;
        }

        torqueDistributionValue = 1f / wheels.Count;

        if (_realtimeView.isOwnedLocallySelf)
        {
            isNetworkInstance = false;
            uIManager = FindObjectOfType<UIManager>();
            uIManager.EnableUI();
            speedDisplay = uIManager.speedometer;
            healthRadialLoader = uIManager.playerHealthRadialLoader;
            IDDisplay.gameObject.SetActive(false);
            IDDisplay = uIManager.playerName;
            boostRadialLoader = uIManager.boostRadialLoader;
            StartCoroutine(BoostCounter());
            StartCoroutine(FireCR());
            InitCam();
            wheelCount = wheels.Count;
            for (int i = 0; i < wheelCount; i++)
            {
                wheels[i].collider.wheelDampingRate = wheels[i].dampingRate;
                wheels[i].collider.ConfigureVehicleSubsteps(5, 12, 15);
                if (wheels[i].trail != null)
                {
                    wheels[i].trail.emitting = false;
                }
            }

            PlayerManager.instance.AddLocalPlayer(transform);
        }
        else
        {
            if (offlineTest)
                InitCam();
            _miniMapRenderer.color = Color.red;
            isNetworkInstance = true;
            muzzleFlash.SetActive(false);
            IDDisplay.gameObject.SetActive(true);
            if (!PlayerManager.instance.networkPlayers.Contains(transform))
            {
                PlayerManager.instance.networkPlayers.Add(transform);
            }
        }
        _currentName = _player.playerName;
        m_fplayerLastHealth = 0f;
        ResetPlayerHealth();
        IDDisplay.SetText(_currentName);
        actualMaxSpeed = maxSpeed / speedDisplayMultiplier;
    }
    private IEnumerator FireCR()
    {
        _bombs++;
        muzzleFlash.SetActive(true);
        StartCoroutine(MuzzleToggle());
        yield return wait;
        readyToFire = true;
    }
    IEnumerator MuzzleToggle()
    {
        yield return muzzleWait;
        muzzleFlash.SetActive(false);
    }
    public IEnumerator UpdateHealth()
    {
        float duration = 0.25f;

        float normalizedTime = 0;

        while (normalizedTime <= 1f)
        {
            normalizedTime += (Time.deltaTime / duration);

            if (healthRadialLoader != null)
            {
                healthRadialLoader.fillAmount = (Mathf.Lerp(m_fplayerLastHealth, _player.playerHealth, normalizedTime)) / _player.maxPlayerHealth;
            }
            yield return null;
        }
        m_fplayerLastHealth = _player.playerHealth;
    }
    public IEnumerator UpdateHealthValue()
    {
        bool _up = true;
        bool _start = true;
        while (m_fplayerLastHealth != _player.playerHealth)
        {
            if (_start)
            {
                _start = false;
                if (m_fplayerLastHealth > _player.playerHealth)
                {
                    _up = false;
                }
            }
            if (_up) m_fplayerLastHealth += Time.deltaTime;
            else m_fplayerLastHealth -= Time.deltaTime;

            if ((_up && m_fplayerLastHealth > _player.playerHealth) || (!_up && m_fplayerLastHealth < _player.playerHealth))
            {
                m_fplayerLastHealth = _player.playerHealth;
            }
            if (healthRadialLoader != null)
            {
                healthRadialLoader.fillAmount = (m_fplayerLastHealth / _player.maxPlayerHealth);
            }
            yield return waitFrame2;
        }
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
                Color _temp = boostRadialLoader.color;
                _temp.a = Mathf.Abs(Mathf.Cos(Time.realtimeSinceStartup));
                boostRadialLoader.color = _temp;
                //boostRadialLoader.enabled = Time.realtimeSinceStartup % 1f > .05f;
            }
            yield return waitFrame;
        }
    }
    private void InitCam()
    {
        chaseCam = GameObject.FindObjectOfType<Camera_Controller>();
        chaseCam.InitCamera(carBody);
    }
    public void ExplosionForce(Vector3 _origin)
    {
        if (!isNetworkInstance)
        {
            carBody.AddExplosionForce(explosionForce, transform.position - _origin, 20f, 1000f);
        }
        else
        {
            if (_player.explosionForce != _origin)
            {
                _player.ChangeExplosionForce(_origin);
            }
        }
        _player.explosionForce = Vector3.zero;
    }
    private void Update()
    {
        if (_currentName != _player.playerName)
            _currentName = _player.playerName;

        if (IDDisplay.text != _currentName)
            IDDisplay.SetText(_currentName);

        if (isNetworkInstance)
        {
            for (int i = 0; i < wheelCount; i++)
            {
                if (wheels[i].model != null)
                {
                    if (wheels[i].collider != null)
                    {
                        wheels[i].collider.enabled = false;
                    }
                }
            }
            return;
        }
        if (transform.position.y < -300)
        {
            transform.position = new Vector3(0f, 45f, 0f);
        }
        if (fireTimer != 1 / fireRate)
        {
            fireTimer = 1 / fireRate;
        }
        if (!offlineTest)
        {
            _realtimeView.RequestOwnership();
            _realtimeTransform.RequestOwnership();
            for (int i = 0; i < wheelCount; i++)
            {
                wheels[i].model.GetComponent<RealtimeView>().RequestOwnership();
                wheels[i].model.GetComponent<RealtimeTransform>().RequestOwnership();
            }
        }

        if (_player.explosionForce != Vector3.zero)
        {
            ExplosionForce(_player.explosionForce);
        }
        trueVelocity = transform.InverseTransformVector(carBody.velocity).z;
        velocity = Mathf.Abs(trueVelocity);
        sidewaysVelocity = Mathf.Abs(transform.InverseTransformVector(carBody.velocity).x);
        inReverse = trueVelocity < 0f;
        ListenForInput();

        currentTorque = verticalInput * torque;

        if (velocity < .333f && verticalInput == 0f)
        {
            isBraking = true;
        }
        if (!experimentalCar)
        {
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
        }
        totalRPM = 0f;
        for (int i = 0; i < wheelCount; i++)
        {
            totalRPM += wheels[i].collider.rpm;
        }
        RPM = totalRPM / wheelCount;

        if (_player != null)
        {
            CheckHealth();
        }
    }
    private void FixedUpdate()
    {
#if UNITY_EDITOR
        carBody.centerOfMass = new Vector3(carBody.centerOfMass.x, centerOfMassY, carBody.centerOfMass.z);
        Debug.DrawLine(transform.TransformPoint(initialCenterOfMass), transform.TransformPoint(carBody.centerOfMass), Color.cyan);
#endif
        // If this CubePlayer prefab is not owned by this client, bail.
        if (isNetworkInstance)
            return;
        RunWheels();
        if (limitTopSpeed)
            carBody.velocity = Vector3.ClampMagnitude(carBody.velocity, actualMaxSpeed);

        Debug.DrawLine(transform.position, transform.position + carBody.velocity, Color.yellow);
    }
    private void CheckHealth()
    {
        if (m_fplayerLastHealth != _player.playerHealth)
        {
            StartCoroutine(UpdateHealthValue());

            if (_player.playerHealth <= 0)
            {
                PlayerDeath();
            }
        }
    }
    private void PlayerDeath()
    {
        isPlayerAlive = false;
        DeathExplosion.SetActive(true);
        if (!experimentalCar)
            GetComponent<Renderer>().material = CarStates[1];
        //DeathExplosion.GetComponent<ParticleSystem>().Play();
        //carBody.AddExplosionForce(200000f, this.transform.position, 20f, 1000f, ForceMode.Impulse);
        verticalInput = 0f;
        horizontalInput = 0f;
        carBody.velocity = Vector3.zero;
        StartCoroutine(RespawnCountDown(5f));
    }
    private IEnumerator RespawnCountDown(float duration)
    {
        yield return new WaitForSeconds(duration);
        DeathExplosion.SetActive(false);
        m_fplayerLastHealth = 0f;
        ResetPlayerHealth();
    }
    private void ResetPlayerHealth()
    {
        isPlayerAlive = true;
        _player.playerHealth = _player.maxPlayerHealth;
        if (!experimentalCar)
            GetComponent<Renderer>().material = CarStates[0];
        StartCoroutine(UpdateHealthValue());
    }
    private void OnDestroy()
    {
        if (isNetworkInstance)
            PlayerManager.instance.RemoveNetworkPlayer(transform);
    }
    private void RunWheels()
    {
        numberOfTiresTouchingGround = 0;

        foreach (Wheel wheel in wheels)
        {
            wheel.collider.ConfigureVehicleSubsteps(3, 10, 14);
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
                    wheel.collider.motorTorque = currentTorque * torqueDistributionValue;
                }
            }

            if (wheel.isSteeringWheel)
            {
                if (inReverse)
                    wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, -horizontalInput * maxSteering, steeringSpeed);
                else
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
                if (drawSkidmark && !isNetworkInstance)
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

        //steering aid
        if (turnAid && numberOfTiresTouchingGround > 1)
            //transform.Rotate(transform.up, turnAidLevel * Time.deltaTime * horizontalInput);
            carBody.AddRelativeTorque(transform.up * horizontalInput * turnAidLevel * Time.deltaTime, ForceMode.VelocityChange);

        if (speedDisplay != null)
        {
            speedDisplay.text = Mathf.RoundToInt((velocity * speedDisplayMultiplier)).ToString();
        }
    }
    private void ListenForInput()
    {
        if (isPlayerAlive)
        {

            verticalInput = Input.GetAxisRaw("Vertical");
            if (clutchTimer < clutchTime && verticalInput > 0f)
            {
                verticalInput *= Mathf.Clamp(clutchCurve.Evaluate(clutchTimer / clutchTime), -1, 1);
                clutchTimer += Time.deltaTime;
            }
            else if (verticalInput <= .01f)
            {
                clutchTimer = 0f;
            }

            if (verticalInput >= 0f)
                horizontalInput = Input.GetAxisRaw("Horizontal");
            else
                horizontalInput = -Input.GetAxisRaw("Horizontal");


            if (Input.GetKeyDown(KeyCode.R) || Input.GetButton("Reset"))//reset
            {
                if (Quaternion.Angle(Quaternion.identity, transform.rotation) > 45f)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + resetHeight, transform.position.z);
                    Vector3 _rotation = transform.rotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
                    carBody.velocity = Vector3.zero;
                    _resets++;
                }
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Lights"))//lights
            {
                lights = !lights;
                RHL.enabled = lights;
                LHL.enabled = lights;
            }

            isBraking = Input.GetKey(KeyCode.Space) || Input.GetButton("Brake");//handbrake;

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Boost"))//boost/dash
            {
                if (boosterReady && numberOfTiresTouchingGround > 0)
                {
                    boosterReady = false;
                    carBody.AddForce(transform.forward * dashForce, ForceMode.VelocityChange);
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetButton("Fire"))
            {
                if (readyToFire)
                {
                    readyToFire = false;
                    _bulletBuffer = Realtime.Instantiate(WeaponProjectile.name,
                    position: _barrelTip.position,
                    rotation: _barrelTip.rotation,
                    ownedByClient: true,
                    useInstance: _realtime);

                    //Old code
                    //_bulletBuffer.GetComponent<Bullet>().isNetworkInstance = false;
                    //_bulletBuffer.GetComponent<Bullet>().Fire(_barrelTip, velocity);

                    _bulletBuffer.GetComponent<WeaponProjectileBase>().isNetworkInstance = false;
                    _bulletBuffer.GetComponent<WeaponProjectileBase>().Fire(_barrelTip, velocity);
                    _bulletBuffer.GetComponent<WeaponProjectileBase>().ProjectileID = ownerID;

                    StartCoroutine(FireCR());
                }

            }

            //AutoDamage Debug
            if (Input.GetKeyDown(KeyCode.L))
            {
                _player.DamagePlayer(5f);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                _player.HealPlayer(5f);
            }
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
