using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;
using UnityEngine.UI;

public class NewCarController : MonoBehaviour
{
    [Space] [Space] [Header("Car Controller Main Settings")]
    public Rigidbody CarRB;

    private float moveInput, turnInput;
    public float fwdSpeed, reverseSpd, turnSpd, turningFwdSpeed;
    public float airDrag, groundDrag;

    public LayerMask groundLayer;
    public float lerpRotationSpeed;
    public float GroundCheckRayLength;

    public float MinVelocityThreshold;
    public float BrakeForce;
    public float MaxSpeed;

    [SerializeField] private bool isGrounded;
    public float extraGravity;
    int wheelCount;
    Quaternion _tempQ;
    float _tempSuspensionDistance;
    public float maxSteeringAngle;
    public float wheelTurnFactor;
    public Transform carBody;
    public float maxZrotation, maxXRotation;
    public float zRotationLERPSpeed, xRotationLERPSpeed;
    public float rotationCooldownTime;
    float currentZ, currentX;
    float XTimer, ZTimer, XFactor, ZFactor;

    [Space] [Space] [Header("Camera and Networking")]
    //Neworking Related Functionalities
    public Realtime _realtime;

    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;
    public int ownerID;
    private ChaseCam followCamera;
    public Transform CameraContainer;
    public bool isNetworkInstance = true;
    public bool offlineTest;


    [Space] [Space] [Header("Loot Based Modifiers")]
    //Does the car need to know about these or does the game manager needs to know about these?
    //Car simply keeps track of what it encounters and talks to game managers to obtain loot or powerups
    public float meleeDamageModifier;

    public float verticalSpdModifier;
    public float turnSpdModifier;

    //Loot Modifiers
    //Engine and weapon projectiles needs to be updated
    public float MaxSpeedModifier, accelerationModifier, HandlingModifier;
    public GameObject LootWeaponProjectile;


    [Space]
    [Space]
    [Header("Weapon Controls")]
    //Weapon Controls
    [SerializeField]
    private GameObject WeaponProjectile;

    private GameObject _bulletBuffer;
    public Transform _barrelTip;
    public float fireRate; //number of bullets fired per second //Weapon Change should affect this variable
    public bool readyToFire = false;
    public GameObject muzzleFlash;
    float fireTimer;

    [Space]
    [Space]
    [Header("UI")]
    //UI Controls
    [SerializeField]
    private UIManager uIManager;

    public string _currentName;

    public TextMeshProUGUI speedDisplay, IDDisplay;

    [Space] [Space] [Header("Health Params")]
    //Health Controls
    public Player _player;

    public Image healthRadialLoader;
    public float m_fplayerLastHealth;
    public GameObject DeathExplosion;
    bool isPlayerAlive;
    public float explosionForce = 2000000f;

    [Space] [Space] [Header("Boost Params")]
    //Boost Controls
    public Image boostRadialLoader;

    public bool enableBoost = true;
    public float boostCooldownTime = 5f;
    public float dashForce;
    public bool boosterReady;
    private float boosterCounter;

    [Space] [Space] [Header("Light Controls")]
    //Light Controls
    public Light RHL;

    public Light LHL;

    private bool lights;

    //Reset Controls
    [Space] public float resetHeight;

    //QA
    [HideInInspector] public int _bombs;

    private WaitForEndOfFrame waitFrame, waitFrame2;
    private WaitForSeconds wait, muzzleWait;

    [HideInInspector] public int _resets;

    public SpriteRenderer _miniMapRenderer;

    [Space] [Header("Suspension and Wheel Settings")]
    public bool identicalSuspension4AW;

    public float suspensionHeight; // these 2 only work if identical suspension for all wheels is true
    public float wheelSize;
    [SerializeField] public ArcadeWheel[] wheels;
    private Coroutine healthChecker;

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
        if (!offlineTest)
        {
            _realtimeView.enabled = true;
            _realtimeTransform.enabled = true;
            fireTimer = 1f / fireRate;
            _player = GetComponent<Player>();
            waitFrame = new WaitForEndOfFrame();
            waitFrame2 = new WaitForEndOfFrame();
            wait = new WaitForSeconds(fireTimer);
            muzzleWait = new WaitForSeconds(.2f);
        }
    }

    void InitCamera()
    {
        followCamera = GameObject.FindObjectOfType<ChaseCam>();
        followCamera.InitCamera(CameraContainer);
    }

    private void CheckIfHasWeapons()
    {
        if (LootWeaponProjectile != null)
        {
            WeaponProjectile = LootWeaponProjectile;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_realtimeView.isOwnedLocallySelf)
        {
            CheckIfHasWeapons();
            isNetworkInstance = false;
            uIManager = FindObjectOfType<UIManager>();
            uIManager.EnableUI();
            //Decouple Sphere Physics from car model
            CarRB.transform.parent = null;
            wheelCount = wheels.Length;

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].originY = wheels[i].wheelT.localPosition.y;
            }

            currentX = carBody.localEulerAngles.x;
            currentZ = carBody.localEulerAngles.z;
            speedDisplay = uIManager.speedometer;
            healthRadialLoader = uIManager.playerHealthRadialLoader;
            IDDisplay = uIManager.playerName;
            IDDisplay.gameObject.SetActive(false);
            boostRadialLoader = uIManager.boostRadialLoader;
            StartCoroutine(BoostCounter());
            StartCoroutine(FireCR());
            InitCamera();
            PlayerManager.instance.AddLocalPlayer(transform);
            PlayerManager.instance.AddExistingPlayers();
        }
        else
        {
            _miniMapRenderer.color = Color.red;
            m_fplayerLastHealth = 0f;
            isNetworkInstance = true;
            IDDisplay.gameObject.SetActive(true);
            CarRB.gameObject.SetActive(false);
            IDDisplay.SetText(_currentName);
            if (!PlayerManager.instance.networkPlayers.Contains(transform))
            {
                PlayerManager.instance.networkPlayers.Add(transform);
                PlayerManager.instance.AddExistingPlayers();
            }
        }

        _currentName = _player.playerName;
        IDDisplay.SetText(_currentName);
        ownerID = _realtimeTransform.ownerIDInHierarchy;
        ResetPlayerHealth();
    }

    void OnValidate()
    {
        if (wheels != null)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].t != null)
                {
                    if (wheels[i].wheelT == null)
                    {
                        wheels[i].wheelT = wheels[i].t.GetChild(0);
                    }

                    if (wheels[i].wheelRT == null)
                    {
                        wheels[i].wheelRT = wheels[i].t.GetComponent<RealtimeTransform>();
                    }

                    if (wheels[i].wheelRTV == null)
                    {
                        wheels[i].wheelRTV = wheels[i].t.GetComponent<RealtimeView>();
                    }

                    if (wheels[i].trail == null)
                    {
                        wheels[i].trail = wheels[i].t.GetChild(1).gameObject;
                    }

                    if (identicalSuspension4AW)
                    {
                        wheels[i].suspensionHeight = suspensionHeight;
                        wheels[i].wheelSize = wheelSize;
                    }
                }
            }
        }
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

            if (_up) m_fplayerLastHealth += Time.deltaTime * 5f;
            else m_fplayerLastHealth -= Time.deltaTime * 5f;

            if ((_up && m_fplayerLastHealth > _player.playerHealth) ||
                (!_up && m_fplayerLastHealth < _player.playerHealth))
            {
                m_fplayerLastHealth = _player.playerHealth;
            }

            if (healthRadialLoader != null)
            {
                healthRadialLoader.fillAmount = (m_fplayerLastHealth / _player.maxPlayerHealth);
            }

            yield return waitFrame2;
        }

        if (_player.playerHealth <= 0)
        {
            PlayerDeath();
        }

        healthChecker = null;
    }

    IEnumerator BoostCounter()
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

    public void ExplosionForce(Vector3 _origin)
    {
        if (!isNetworkInstance)
        {
            CarRB.AddExplosionForce(explosionForce, transform.position - _origin, 20f, 1000f);
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

    // Update is called once per frame
    void Update()
    {
        if (!isNetworkInstance)
        {
            _realtimeView.RequestOwnership();
            _realtimeTransform.RequestOwnership();

            if (_currentName != _player.playerName)
                _currentName = _player.playerName;

            if (IDDisplay.text != _currentName)
                IDDisplay.SetText(_currentName);

            for (int i = 0; i < wheelCount; i++)
            {
                wheels[i].wheelRTV.RequestOwnership();
                wheels[i].wheelRT.RequestOwnership();
            }

            if (_player.explosionForce != Vector3.zero)
            {
                ExplosionForce(_player.explosionForce);
            }

            //disable controls when player is dead
            if (isPlayerAlive)
            {
                DetectInput();
                RotationCheck();
                TurnTheWheels();
            }

            DragCheck();
            GroundCheck();
            transform.position = CarRB.transform.position;
        }

        if (_player != null)
        {
            CheckHealth();
        }
    }

    private void CheckHealth()
    {
        Debug.LogWarning("Health Comparison | local: " + m_fplayerLastHealth + " | model: " + _player.playerHealth);
        if (Mathf.RoundToInt(m_fplayerLastHealth) != Mathf.RoundToInt(_player.playerHealth))
        {
            if (healthChecker == null)
                healthChecker = StartCoroutine(UpdateHealthValue());
        }
    }

    private void PlayerDeath()
    {
        isPlayerAlive = false;
        moveInput = 0;
        turnInput = 0;
        DeathExplosion.SetActive(true);
        CarRB.AddExplosionForce(150f, this.CarRB.transform.position, 20f, 500f, ForceMode.Impulse);
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

        //Spawn in player animation
        CarRB.position = new Vector3(transform.position.x, transform.position.y + resetHeight, transform.position.z);
        Vector3 _rotation = CarRB.rotation.eulerAngles;
        CarRB.transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        CarRB.velocity = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (isNetworkInstance)
            PlayerManager.instance.RemoveNetworkPlayer(transform);
        if (CarRB != null)
            Destroy(CarRB.gameObject);
    }

    private void FixedUpdate()
    {
        if (!isNetworkInstance)
        {
            if (speedDisplay != null)
            {
                speedDisplay.text = Mathf.RoundToInt(LocalVelocity()).ToString();
            }

            CancelResidualVelocity();
            MaxSpeedCheck();
            if (isGrounded)
            {
                CarRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
            }
            else
            {
                //Increase artifical gravity when in freefall
                //CarRB.AddForce(transform.forward * 5f, ForceMode.Force);
                CarRB.AddForce(-Vector3.up * extraGravity * 100f);
            }
        }
    }

    private void CancelResidualVelocity()
    {
        if (moveInput == 0 && turnInput == 0 & CarRB.velocity.magnitude < MinVelocityThreshold)
        {
            Vector3 oppositeForce = CarRB.transform.InverseTransformDirection(-CarRB.velocity);

            if (CarRB.velocity.magnitude > MinVelocityThreshold)
            {
                CarRB.AddRelativeForce(oppositeForce * BrakeForce, ForceMode.Force);
            }
            else
            {
                CarRB.velocity = Vector3.zero;
            }
        }
    }

    private void MaxSpeedCheck()
    {
        if (CarRB.velocity.magnitude > (MaxSpeed * (1 + MaxSpeedModifier)))
        {
            CarRB.velocity = CarRB.velocity.normalized * (MaxSpeed * (1 + MaxSpeedModifier));
        }
    }

    private float ProjectileVelocity(Vector3 velocity)
    {
        float trueVelocity = transform.InverseTransformVector(velocity).z;
        float projectileVelocity = Mathf.Abs(trueVelocity);
        return projectileVelocity;
    }

    private void TurnTheWheels()
    {
        for (int i = 0; i < wheelCount; i++)
        {
            Debug.DrawRay(wheels[i].t.position, -wheels[i].t.up, Color.white);
            Physics.Raycast(wheels[i].t.position, -wheels[i].t.up, out RaycastHit hit, Mathf.Infinity, groundLayer);
            _tempSuspensionDistance = Mathf.Clamp(hit.distance, 0, wheels[i].suspensionHeight);
            wheels[i].trail.GetComponent<TrailRenderer>().emitting =
                hit.distance < wheels[i].suspensionHeight + wheels[i].wheelSize;

            Debug.DrawLine(wheels[i].t.position, hit.point, Color.red);

            wheels[i].wheelT.localPosition = new Vector3(wheels[i].wheelT.localPosition.x,
                wheels[i].originY - _tempSuspensionDistance + wheels[i].wheelSize, wheels[i].wheelT.localPosition.z);

            if (wheels[i].isSteeringWheel)
            {
                _tempQ = wheels[i].t.localRotation;
                wheels[i].wheelT.localEulerAngles = new Vector3(_tempQ.eulerAngles.x, turnInput * maxSteeringAngle,
                    _tempQ.eulerAngles.z);
            }

            if (wheels[i].isPowered)
            {
                if (moveInput != 0)
                {
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * moveInput * wheelTurnFactor);
                }
                else
                {
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * wheelTurnFactor);
                }
            }
            else
            {
                wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * wheelTurnFactor);
            }
        }
    }

    void DetectInput()
    {
        if (moveInput != Input.GetAxis("Vertical") && moveInput == 0)
        {
            XTimer = rotationCooldownTime;
        }

        if (turnInput == 0 && Input.GetAxis("Horizontal") != turnInput)
        {
            ZTimer = rotationCooldownTime;
        }

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (XTimer > 0f)
        {
            XTimer -= Time.deltaTime;
            XFactor = (XTimer / rotationCooldownTime) * maxXRotation;
            currentX = Mathf.Clamp(
                Mathf.LerpAngle(currentX, -moveInput * maxXRotation, xRotationLERPSpeed * Time.deltaTime), -XFactor,
                XFactor);
        }

        if (ZTimer > 0f)
        {
            ZTimer -= Time.deltaTime;
            ZFactor = (ZTimer / rotationCooldownTime) * maxZrotation;
            currentZ = Mathf.Clamp(
                Mathf.LerpAngle(currentZ, turnInput * maxZrotation, zRotationLERPSpeed * Time.deltaTime), -ZFactor,
                ZFactor);
        }

        carBody.localEulerAngles = new Vector3(currentX, carBody.localEulerAngles.y, currentZ);

        if (moveInput > 0)
        {
            moveInput *= (fwdSpeed * (1 + accelerationModifier));
        }
        else
        {
            moveInput *= (reverseSpd * (1 + accelerationModifier));
        }

        //Weapon Controls
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

                _bulletBuffer.GetComponent<WeaponProjectileBase>().isNetworkInstance = false;
                _bulletBuffer.GetComponent<WeaponProjectileBase>().Fire(_barrelTip, ProjectileVelocity(CarRB.velocity));
                _bulletBuffer.GetComponent<WeaponProjectileBase>().originOwnerID = ownerID;

                StartCoroutine(FireCR());
            }
        }

        //Need to add reset timer to avoid spamming
        if (Input.GetKeyDown(KeyCode.R) || Input.GetButton("Reset")) //reset
        {
            if (Quaternion.Angle(Quaternion.identity, transform.rotation) > 45f)
            {
                CarRB.position = new Vector3(transform.position.x, transform.position.y + resetHeight,
                    transform.position.z);
                Vector3 _rotation = CarRB.rotation.eulerAngles;
                CarRB.transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
                CarRB.velocity = Vector3.zero;
                _resets++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Boost")) //boost/dash
        {
            if (boosterReady && isGrounded)
            {
                boosterReady = false;
                CarRB.AddForce(transform.forward * dashForce, ForceMode.VelocityChange);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Lights")) //lights
        {
            lights = !lights;
            RHL.enabled = lights;
            LHL.enabled = lights;
        }

        //AutoDamage Debug
        //TO REMOVE in testing and final builds
        if (Input.GetKeyDown(KeyCode.L))
        {
            _player.DamagePlayer(5f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            _player.HealPlayer(5f);
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit ground, GroundCheckRayLength,
            groundLayer);
        Debug.DrawLine(transform.position, ground.point, Color.cyan);

        Physics.Raycast(transform.position, -transform.up, out RaycastHit rotationAlignment,
            (GroundCheckRayLength + 2f), groundLayer);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            (Quaternion.FromToRotation(transform.up, rotationAlignment.normal)
             * transform.rotation), Time.deltaTime * lerpRotationSpeed);
    }

    void DragCheck()
    {
        if (isGrounded)
        {
            CarRB.drag = groundDrag;
        }
        else
        {
            CarRB.drag = airDrag;
        }
    }

    private void RotationCheck()
    {
        //Debug.Log(" Vertical input is: " + moveInput);
        //if only pressing turning there should be a small amount of acceleration/speed so car is not turning on its own without momentum
        if (isGrounded)
        {
            if (turnInput != 0 && moveInput == 0)
            {
                //Debug.Log("Stationary Turning");
                float StationaryRotation = (1 + HandlingModifier) * turnInput * turnSpd * Time.deltaTime;
                CarRB.AddForce(transform.forward * turningFwdSpeed, ForceMode.Acceleration);
                transform.Rotate(0, StationaryRotation, 0, Space.World);
            }
            else
            {
                float motionRotation = ((1 + HandlingModifier) * turnInput * turnSpd * Time.deltaTime *
                                        Input.GetAxisRaw("Vertical"));
                transform.Rotate(0, motionRotation, 0, Space.World);
            }
        }
    }

    public float LocalVelocity()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(CarRB.velocity);
        return Mathf.Abs(localVelocity.z);
    }

    //Framerate aware damping
    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    //Weapon Firing Codes
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
}

[Serializable]
public struct ArcadeWheel
{
    public Transform t; //connection point to the car
    public Transform wheelT; //transform of actual wheel
    public RealtimeView wheelRTV;
    public RealtimeTransform wheelRT;
    public GameObject trail;
    public bool isPowered;
    public bool isSteeringWheel;
    public float suspensionHeight;
    public float wheelSize; //radius of the wheel (r)
    [HideInInspector] public float originY;
}