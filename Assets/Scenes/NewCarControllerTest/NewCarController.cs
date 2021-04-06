using System;
using System.Collections;
using System.Collections.Generic;
using Items.TempMods;
using UnityEngine;
using Normal.Realtime;
using TMPro;
using UnityEngine.UI;

public class NewCarController : MonoBehaviour
{
    [Space] [Space] [Header("Car Controller Main Settings")]
    public Rigidbody CarRB;

    private float moveInput, turnInput;
    public float acceleration, reverseAccel, turnSpd, turningFwdSpeed;
    public float airDrag, groundDrag;

    public LayerMask groundLayer;
    public float lerpRotationSpeed;
    public float GroundCheckRayLength;

    public float MinVelocityThreshold;
    public float BrakeForce;
    public float MaxSpeed;

    [SerializeField] private List<CarPhysicsParamsSObj> m_carDataContainer = new List<CarPhysicsParamsSObj>();
    [SerializeField] private CarPhysicsParamsSObj m_currentPhysicsSet = null;
    private int dataParamsIndex = 0;

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

    public RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;
    private ChaseCam followCamera;
    public Transform CameraContainer, lookAtTarget, forwardCamera, rearCamera;
    public bool offlineTest;

    bool resetReverseView = false;
    bool CoroutineReset = false;


    [Space] [Space] [Header("Loot Based Modifiers")]
    //Does the car need to know about these or does the game manager needs to know about these?
    //Car simply keeps track of what it encounters and talks to game managers to obtain loot or powerups
    public float meleeDamageModifier;

    //Loot Modifiers
    //Engine and weapon projectiles need to be updated
    private float maxSpeedModifier, meleePower;
    [SerializeField] private LootManager lootManager;

    //Temporary mods
    public float tempSpeedModifier = 0f;
    public float tempDefenseModifier = 0f;
    public float tempTruckDamageModifier = 0f;
    public float tempBoostModifier = 0;

    [Space]
    [Space]
    [Header("Weapon Controls")]
    //Weapon Controls
    //This is the default weapon the truck fires
    [SerializeField]
    private GameObject PrimaryWeaponProjectile;

    [SerializeField] private GameObject SecondaryWeaponProjectile;
    [SerializeField] private float startingAmmo = 5f;
    private GameObject _bulletBuffer;
    public Transform _barrelTip;
    public float primaryfireRate = 1;
    public float secondaryfireRate = 1; //number of bullets fired per second //Weapon Change should affect this variable
    public bool readyToFire = false;
    public GameObject muzzleFlash;
    public float currentAmmo;

    [SerializeField] private float ammoEfficiency;

    [SerializeField] private BarrelShaker m_BarrelShaker;

    private float primaryAmmo;
    private float secondaryAmmo;
    private float temptAmmo;

    public float primaryFireTimer;
    public float secondayFireTimer;
    public float weaponType;

    //For primary weapon w/ infinite ammo
    public bool Overheat;
    public float heatLevel;
    public float maxHeatThreshold;
    public float OverheatCoolTimer;

    float savedPrimaryFireTimer;

    //Make default weapon as a readable weapon stats with firerate and 
    //Sobj
    private GameObject savedWeaponProjectile;
    private float savedWeaponFireRate;
    private float savedWeaponAmmo;
    private float savedTempDamageRate;

    [SerializeField] private TurretAutoAim turretAim;

    [Space]
    [Space]
    [Header("UI")]
    //UI Controls
    [SerializeField]
    private UIManager uIManager;

    public string _currentName;

    public TextMeshProUGUI speedDisplay, IDDisplay;

    public Image OverheatMeter;
    public GameObject OverheatMeterObj;
    public GameObject OverHeatNotice;
    public GameObject WeaponSwitcherUI;

    [Space] [Space] [Header("Health Params")]
    //Health Controls
    public Player _player;

    public Image healthRadialLoader;
    public float m_fplayerLastHealth;
    public GameObject DeathExplosion;
    public bool isPlayerAlive = true;
    public float explosionForce = 2000000f;
    public float resetHeight;

    public float damageFeedbackDuration = .33f; //duration of camera shake
    private Coroutine cR_damageEffect;
    private WaitForEndOfFrame waitFrameDamageEffect;
    public CanvasGroup damageIndicatorCanvasGroup;


    [Space] [Space] [Header("Boost Params")]
    //Boost Controls
    public Image boostRadialLoader;

    public bool enableBoost = true;
    public float boostCooldownTime = 5f;
    public float dashForce;
    public bool boosterReady, isBoosting;
    private float boosterCounter;

    [Space] [Space] [Header("Light Controls")]
    //Light Controls
    public Light RHL;

    public Light LHL;

    private bool lights;


    [SerializeField] private int PhysicsBuildIndex = 0;

    //QA
    [HideInInspector] public int _bombs;

    private WaitForEndOfFrame waitFrame, waitFrame2, waitFrame3;
    private WaitForSeconds primaryWait, secondaryWait, muzzleWait;

    [HideInInspector] public int _resets;

    [Space] [Header("Suspension and Wheel Settings")]
    public bool identicalSuspension4AW;

    public float suspensionHeight; // these 2 only work if identical suspension for all wheels is true
    public float wheelSize;
    [SerializeField] public ArcadeWheel[] wheels;
    private Coroutine healthAnimator;
    private float _tempHealth;

    public ParticleSystem[] boostParticles;
    private int theKiller = -1;
    public GameObject lootIndicator;
    private Text lootIndicatorCountDisplay;
    public AudioPlayer boostSound;
    private LayeredAudioPlayer deathSound;
    private WaitForSeconds wait1sec;

    public void ToggleController(bool state)
    {
        isPlayerAlive = state;
        CarRB.isKinematic = !state;
    }

    public void RegisterDamage(float damage, RealtimeView realtimeView)
    {
        theKiller = realtimeView
            .ownerIDInHierarchy; //everyone is the potential killer until the target survives the hit
        if (_realtimeView.isOwnedLocallyInHierarchy)
            _player.DamagePlayer(damage);
        if (theKiller == _realtimeView.ownerIDInHierarchy)
        {
            var difference = transform.position - realtimeView.transform.position;
            _player.ChangeExplosionForce(difference);
            ExplosionForce(difference);
        }

        //Debug.LogWarning(PlayerManager.instance.PlayerName(_realtimeView.ownerIDInHierarchy) + " was " + (_player.playerHealth - damage <= 0 ? "killed" : "damaged") + " by " + PlayerManager.instance.PlayerName(theKiller));
    }

    private void GetPhysicsParamsBasedOnBuild()
    {
        switch (PhysicsBuildIndex)
        {
            case 0:
                m_carDataContainer.Add(lootManager.CarPhysicsParams[0]);
                m_currentPhysicsSet = lootManager.CarPhysicsParams[0];
                break;
            case 1:
                m_carDataContainer.Add(lootManager.CarPhysicsParams[1]);
                m_currentPhysicsSet = lootManager.CarPhysicsParams[1];
                break;
            case 2:
                m_carDataContainer.Add(lootManager.CarPhysicsParams[2]);
                m_currentPhysicsSet = lootManager.CarPhysicsParams[2];
                break;
        }
    }

    private void Awake()
    {
        ToggleController(false);
        wait1sec = new WaitForSeconds(1f);
        lootManager = FindObjectOfType<LootManager>();
        GetPhysicsParamsBasedOnBuild();
        if (lootIndicator != null)
        {
            lootIndicator.SetActive(false);
            lootIndicatorCountDisplay = lootIndicator.GetComponentInChildren<Text>();
        }

        waitFrameDamageEffect = new WaitForEndOfFrame();
        if (!offlineTest)
        {
            _realtime = FindObjectOfType<Realtime>();
            _realtimeView = GetComponent<RealtimeView>();
            _realtimeTransform = GetComponent<RealtimeTransform>();
            _realtimeView.enabled = true;
            _realtimeTransform.enabled = true;
        }

        SetWeaponFireRates();

        _player = GetComponent<Player>();
        waitFrame = new WaitForEndOfFrame();
        waitFrame2 = new WaitForEndOfFrame();
        waitFrame3 = new WaitForEndOfFrame();
        primaryWait = new WaitForSeconds(primaryFireTimer);
        secondaryWait = new WaitForSeconds(secondayFireTimer);
        muzzleWait = new WaitForSeconds(.2f);
        turretAim = GetComponentInChildren<TurretAutoAim>();
        SyncPhysicsParamsData();
    }

    void ObtainCorrectShaker()
    {
        BarrelShaker[] shakers = GetComponentsInChildren<BarrelShaker>();

        for (int i = 0; i < shakers.Length; i++)
        {
            if (shakers[i].isActiveAndEnabled)
            {
                m_BarrelShaker = shakers[i];
            }
        }
    }

    //For tuning physics and car physics params
    void SyncPhysicsParamsData()
    {
        m_currentPhysicsSet =
            m_carDataContainer[dataParamsIndex];

        meleeDamageModifier = m_currentPhysicsSet.f_meleePower;
        maxSpeedModifier = m_currentPhysicsSet.f_topSpd;
        acceleration = m_currentPhysicsSet.f_acceleration;
        reverseAccel = m_currentPhysicsSet.f_ReverseAccel;
        turnSpd = m_currentPhysicsSet.f_TurnSpd;
        turningFwdSpeed = m_currentPhysicsSet.f_TurnFwdSpd;
        BrakeForce = m_currentPhysicsSet.f_BrakeForce;
        extraGravity = m_currentPhysicsSet.f_Gravity;
        _player.maxPlayerHealth = m_currentPhysicsSet.f_maxPlayerHealth;
        _player.armourDefenseModifier = m_currentPhysicsSet.f_defenseForce;
        ammoEfficiency = m_currentPhysicsSet.f_ammoEfficiency;
        CarRB.mass = m_currentPhysicsSet.f_rbWeight;
        boostCooldownTime = m_currentPhysicsSet.f_boostTimer;
        dashForce = m_currentPhysicsSet.f_boostForce;
    }

    void CycleSyncPhysicsParamsData()
    {
        dataParamsIndex++;
        dataParamsIndex %= m_carDataContainer.Count;

        m_currentPhysicsSet =
            m_carDataContainer[dataParamsIndex];

        meleeDamageModifier = m_currentPhysicsSet.f_meleePower;
        maxSpeedModifier = m_currentPhysicsSet.f_topSpd;
        acceleration = m_currentPhysicsSet.f_acceleration;
        reverseAccel = m_currentPhysicsSet.f_ReverseAccel;
        turnSpd = m_currentPhysicsSet.f_TurnSpd;
        turningFwdSpeed = m_currentPhysicsSet.f_TurnFwdSpd;
        BrakeForce = m_currentPhysicsSet.f_BrakeForce;
        extraGravity = m_currentPhysicsSet.f_Gravity;
        _player.maxPlayerHealth = m_currentPhysicsSet.f_maxPlayerHealth;
        _player.armourDefenseModifier = m_currentPhysicsSet.f_defenseForce;
        ammoEfficiency = m_currentPhysicsSet.f_ammoEfficiency;
        CarRB.mass = m_currentPhysicsSet.f_rbWeight;
        boostCooldownTime = m_currentPhysicsSet.f_boostTimer;
        dashForce = m_currentPhysicsSet.f_boostForce;
    }

    public CarPhysicsParamsSObj GetCarParams()
    {
        if (m_carDataContainer.Count != 0)
        {
            return m_carDataContainer[0];
        }
        else
        {
            return null;
        }
    }

    void SetWeaponFireRates()
    {
        if (PrimaryWeaponProjectile != null)
        {
            primaryfireRate = PrimaryWeaponProjectile.GetComponent<WeaponProjectileBase>().weaponFireRate;
            primaryFireTimer = 1f / primaryfireRate;
        }

        if (SecondaryWeaponProjectile != null)
        {
            secondaryfireRate = SecondaryWeaponProjectile.GetComponent<WeaponProjectileBase>().weaponFireRate;
            secondayFireTimer = 1f / secondaryfireRate;
        }
    }

    void InitCamera()
    {
        followCamera = FindObjectOfType<ChaseCam>();
        followCamera.InitCamera(CameraContainer, lookAtTarget);
    }

    //Use this to apply weapons for loot
    public void SetCurrentWeapon(GameObject PermanentLootWeaponProjectile, float PermaFireRatet, float PermaDmgMod,
        float efficiency)
    {
        SecondaryWeaponProjectile = PermanentLootWeaponProjectile;
        secondaryfireRate = PermaFireRatet;
        secondayFireTimer = 1f / secondaryfireRate;
        secondaryWait = new WaitForSeconds(secondayFireTimer);
        ammoEfficiency = efficiency;
        //tempTruckDamageModifier = PermaDmgMod;
        //Permanent Weapon starts off as 0
    }

    private void SwitchWeaponsDuringGame(GameObject LootWeaponProjectile, float lootFireRate, float damageModifier)
    {
        if (PrimaryWeaponProjectile != null)
        {
            //Cache data for existing weapon in use
            savedWeaponProjectile = SecondaryWeaponProjectile;
            savedWeaponFireRate = secondaryfireRate;
            savedWeaponAmmo = currentAmmo;
            savedTempDamageRate = tempTruckDamageModifier;
        }

        SecondaryWeaponProjectile = LootWeaponProjectile;
        secondaryfireRate = lootFireRate;
        secondayFireTimer = 1f / secondaryfireRate;
        secondaryWait = new WaitForSeconds(secondayFireTimer);
        tempTruckDamageModifier = damageModifier;

        temptAmmo = startingAmmo;
        currentAmmo = temptAmmo;
        WeaponProjectileBase LootWeaponBase = LootWeaponProjectile.GetComponent<WeaponProjectileBase>();
        if (LootWeaponBase != null)
        {
            uIManager.SwitchProjectileDisplayInfo(LootWeaponBase.ProjectileToDisplay, (int) currentAmmo);
        }

        if (weaponType == 0)
        {
            weaponType++;
            weaponType %= 2;
        }
    }

    private void SwitchBackToSavedWeapon()
    {
        //Run this when the ammo runs out on the temp weapon
        SecondaryWeaponProjectile = savedWeaponProjectile;
        secondaryfireRate = savedWeaponFireRate;
        secondayFireTimer = 1f / secondaryfireRate;
        secondaryWait = new WaitForSeconds(secondayFireTimer);
        currentAmmo = savedWeaponAmmo;

        WeaponProjectileBase savedWeaponBase = savedWeaponProjectile.GetComponent<WeaponProjectileBase>();

        if (savedWeaponBase != null)
        {
            uIManager.SwitchProjectileDisplayInfo(savedWeaponBase.ProjectileToDisplay, (int) currentAmmo);
        }

        ResetSavedWeapon();
    }

    private void ResetSavedWeapon()
    {
        savedWeaponProjectile = null;
        savedWeaponAmmo = 0;
        savedWeaponFireRate = 0;
        savedTempDamageRate = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CR_CosmeticHealthAnimator());
        boostSound = GetComponent<AudioPlayer>();
        deathSound = GetComponent<LayeredAudioPlayer>();
        if (_realtimeView.isOwnedLocallyInHierarchy)
        {
            CreateMeleeEntity();
            uIManager = FindObjectOfType<UIManager>();
            if (uIManager != null)
            {
                uIManager.EnableUI();
                speedDisplay = uIManager.speedometer;
                healthRadialLoader = uIManager.playerHealthRadialLoader;
                IDDisplay.gameObject.SetActive(false);
                IDDisplay = uIManager.playerName;
                boostRadialLoader = uIManager.boostRadialLoader;
                OverheatMeter = uIManager.OverheatMeter;
                OverheatMeterObj = uIManager.OverheatMeterObj;
                OverHeatNotice = uIManager.OverheatNotice;
                WeaponSwitcherUI = uIManager.WeaponSwitchIcon;
                damageIndicatorCanvasGroup = uIManager.damageIndicatorCanvasGroup;
            }
            OverheatMeterObj.SetActive(true);

            //Decouple Sphere Physics from car model
            CarRB.transform.parent = null;
            wheelCount = wheels.Length;

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].originY = wheels[i].wheelT.localPosition.y;
            }

            currentX = carBody.localEulerAngles.x;
            currentZ = carBody.localEulerAngles.z;

            StartCoroutine(FirePrimaryCR());

            if (SecondaryWeaponProjectile)
            {
                StartCoroutine(FireSecondaryCR());
            }

            InitCamera();
            if (!offlineTest)
            {
                StartCoroutine(BoostCounter());
                if (healthAnimator == null)
                {
                    healthAnimator = StartCoroutine(CR_HealthAnimator());
                }

                PlayerManager.instance.UpdateExistingPlayers();
            }

            WeaponProjectileBase PrimaryWeaponBase = PrimaryWeaponProjectile.GetComponent<WeaponProjectileBase>();
            uIManager.SwitchProjectileDisplayInfo(PrimaryWeaponBase.ProjectileToDisplay, 999);

            //pick a start position and go there
            var playerCount = FindObjectsOfType<Player>().Length;
            transform.LookAt(FindObjectOfType<CountdownLights>().transform);
        }
        else
        {
            CarRB.gameObject.SetActive(false);
            IDDisplay.gameObject.SetActive(true);
            if (!PlayerManager.instance.networkPlayers.Contains(transform))
            {
                PlayerManager.instance.AddNetworkPlayer(transform);
            }

            if (!offlineTest)
            {
                _currentName = _player.playerName;
                ResetPlayerHealth();
            }
        }

        IDDisplay.SetText(_player.playerName);
        WeaponSwitcherUI.SetActive(false);
        OverHeatNotice.gameObject.SetActive(false);
        OverheatMeterObj.SetActive(_realtimeView.isOwnedLocallyInHierarchy);
        ObtainCorrectShaker();
    }

    private void CreateMeleeEntity()
    {
        GameObject _temp = Realtime.Instantiate("Melee",
            position: transform.position,
            rotation: transform.rotation,
            ownedByClient: true,
            preventOwnershipTakeover: false,
            destroyWhenOwnerOrLastClientLeaves: true,
            useInstance: _realtime);
        _temp.GetComponent<Melee>().Setup(this);
    }

    public void DamageFeedback()
    {
        if (_realtimeView.isOwnedRemotelyInHierarchy) return;
        if (cR_damageEffect != null)
        {
            StopCoroutine(cR_damageEffect);
        }

        cR_damageEffect = StartCoroutine(CR_DamageEffect());
    }

    IEnumerator CR_DamageEffect()
    {
        float _temp = damageFeedbackDuration;
        while (_temp > 0)
        {
            _temp -= Time.deltaTime;
            damageIndicatorCanvasGroup.alpha = _temp / damageFeedbackDuration;
            yield return waitFrameDamageEffect;
        }

        damageIndicatorCanvasGroup.alpha = 0f;
    }

    // void OnValidate()
    // {
    //     if (wheels != null)
    //     {
    //         for (int i = 0; i < wheels.Length; i++)
    //         {
    //             if (wheels[i].t != null)
    //             {
    //                 if (wheels[i].wheelT == null)
    //                 {
    //                     wheels[i].wheelT = wheels[i].t.GetChild(0);
    //                 }
    //
    //                 if (wheels[i].wheelRT == null)
    //                 {
    //                     wheels[i].wheelRT = wheels[i].t.GetComponent<RealtimeTransform>();
    //                 }
    //
    //                 if (wheels[i].wheelRTV == null)
    //                 {
    //                     wheels[i].wheelRTV = wheels[i].t.GetComponent<RealtimeView>();
    //                 }
    //
    //                 if (wheels[i].trail == null)
    //                 {
    //                     wheels[i].trail = wheels[i].t.GetChild(1).gameObject;
    //                 }
    //
    //                 if (identicalSuspension4AW)
    //                 {
    //                     wheels[i].suspensionHeight = suspensionHeight;
    //                     wheels[i].wheelSize = wheelSize;
    //                 }
    //             }
    //         }
    //     }
    // }

    public IEnumerator CR_HealthAnimator()
    {
        yield return new WaitForSeconds(5f);

        while (true)
        {
            if (healthRadialLoader != null)
            {
                _tempHealth = Mathf.Lerp(_tempHealth, m_fplayerLastHealth, Time.deltaTime * 10f);
                healthRadialLoader.fillAmount = _tempHealth / _player.maxPlayerHealth;
            }

            yield return waitFrame2;
        }
    }

    IEnumerator CR_CosmeticHealthAnimator()
    {
        yield return new WaitForSeconds(10f);

        while (true)
        {
            if (_player.playerHealth <= 0 && isPlayerAlive)
            {
                PlayerDeath();
            }

            yield return waitFrame3;
        }
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
                    boostRadialLoader.fillAmount = boosterCounter / (boostCooldownTime * (1 - tempBoostModifier));
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
        if (_realtimeView.isOwnedLocallyInHierarchy)
        {
            CarRB.AddExplosionForce(explosionForce, transform.position - _origin, 20f, 1000f);
            if (_player.explosionForce != _origin)
            {
                _player.ChangeExplosionForce(_origin);
                _player.explosionForce = Vector3.zero;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.counter.current <= 0)
        {
            ToggleController(true);
        }

        if (!Mathf.Approximately(m_fplayerLastHealth, _player.playerHealth))
        {
            UpdateHealth();
        }

        if (_realtimeView.isOwnedLocallyInHierarchy)
        {
            lootIndicator.SetActive(false);
            if (!offlineTest)
            {
                _realtimeView.RequestOwnership();
                _realtimeTransform.RequestOwnership();

                if (IDDisplay.text != _currentName)
                    IDDisplay.SetText(_currentName);

                for (int i = 0; i < wheelCount; i++)
                {
                    wheels[i].wheelRTV.RequestOwnership();
                    wheels[i].wheelRT.RequestOwnership();
                }
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
            LerpFallCorrection();
            transform.position = Vector3.Lerp(transform.position, CarRB.transform.position, 6.66f);
            if (weaponType == 1)
            {
                uIManager.UpdateAmmoCount((int) currentAmmo);
            }
        }
        else
        {
            if (lootIndicator != null) lootIndicator.SetActive(false);
            if (lootIndicator != null)
            {
                if (_player == null) return;
                if (_player.statsEntity == null) return;
                var lootCount = _player.statsEntity._loot;
                if (lootCount > 0)
                {
                    lootIndicator.SetActive(true);
                    lootIndicatorCountDisplay.text = lootCount.ToString();
                }
                else lootIndicator.SetActive(false);
            }
        }
    }

    public void ResetTempModifiers()
    {
        tempTruckDamageModifier = 0;
        tempSpeedModifier = 0;
        tempDefenseModifier = 0;
        tempBoostModifier = 0;
    }

    void UpdateHealth()
    {
        _tempHealth = m_fplayerLastHealth;
        m_fplayerLastHealth = _player.playerHealth;
    }

    private void PlayerDeath()
    {
        if (isPlayerAlive)
        {
            deathSound.Play();
            isPlayerAlive = false;
            moveInput = 0;
            turnInput = 0;
            DeathExplosion.SetActive(true);
            if (theKiller > -1 && theKiller != _realtimeView.ownerIDInHierarchy
            ) //prevent self kill and ghost killing (player dies for a different reason but points are registered to the player who had killed them last, in the past)
            {
                StatsManager.instance.RegisterKill(theKiller);
                theKiller = -1;
            }

            StartCoroutine(RespawnCountDown(5f));
        }
    }

    private IEnumerator RespawnCountDown(float duration)
    {
        var loop = Convert.ToInt32(duration);
        for (int i = 0; i < loop; i++)
        {
            yield return wait1sec;
            deathSound.Play();
        }

        DeathExplosion.SetActive(false);
        ResetPlayerHealth();
    }

    private void ResetPlayerHealth()
    {
        _player.ResetHealth();
        UpdateHealth();
        //Spawn in player animation
        CarRB.position = new Vector3(transform.position.x, transform.position.y + resetHeight, transform.position.z);
        Vector3 _rotation = CarRB.rotation.eulerAngles;
        CarRB.transform.rotation = Quaternion.Euler(0f, _rotation.y, 0f);
        CarRB.velocity = Vector3.zero;
        isPlayerAlive = true;
    }

    private void OnDestroy()
    {
        if (_realtimeView.isOwnedRemotelyInHierarchy && !offlineTest)
            PlayerManager.instance.RemoveNetworkPlayer(transform);
        if (CarRB != null)
            Destroy(CarRB.gameObject);
    }

    private void FixedUpdate()
    {
        if (_realtimeView.isOwnedLocallyInHierarchy)
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
        if (CarRB.velocity.magnitude > (MaxSpeed * (1 + maxSpeedModifier)))
        {
            CarRB.velocity = CarRB.velocity.normalized * (MaxSpeed * (1 + maxSpeedModifier));
        }
    }

    private float ProjectileVelocity(Vector3 velocity)
    {
        float trueVelocity = transform.InverseTransformVector(velocity).z;
        float projectileVelocity = Mathf.Abs(trueVelocity);
        return projectileVelocity;
    }

    private IEnumerator CheckSwitchUI()
    {
        while (!readyToFire)
        {
            yield return new WaitForSeconds(1f);
        }

        //yield return new WaitForSeconds(1f);
        WeaponSwitcherUI.SetActive(false);
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
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * moveInput * -wheelTurnFactor);
                }
                else
                {
                    wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * -wheelTurnFactor);
                }
            }
            else
            {
                wheels[i].wheelT.Rotate(Vector3.right * Time.deltaTime * LocalVelocity() * -wheelTurnFactor);
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
            moveInput *= (150 * (1 + acceleration));
        }
        else
        {
            moveInput *= (150 * (1 + reverseAccel));
        }

        //Weapon Controls
        if (!offlineTest)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetButton("Fire"))
            {
                switch (weaponType)
                {
                    case 0:

                        if (readyToFire && !Overheat)
                        {
                            readyToFire = false;

                            if (m_BarrelShaker != null)
                                m_BarrelShaker.StartShake();

                            _bulletBuffer = Realtime.Instantiate(PrimaryWeaponProjectile.name,
                                position: _barrelTip.position,
                                rotation: _barrelTip.rotation,
                                ownedByClient: true,
                                preventOwnershipTakeover: true,
                                destroyWhenOwnerOrLastClientLeaves: true,
                                useInstance: _realtime);

                            WeaponProjectileBase PrimaryWeaponBase = _bulletBuffer.GetComponent<WeaponProjectileBase>();

                            PrimaryWeaponBase.Fire(_barrelTip, ProjectileVelocity(CarRB.velocity));
                            PrimaryWeaponBase.truckDamageTempModifier = tempTruckDamageModifier;
                            PrimaryWeaponBase.statEntity = _player.statsEntity;

                            StartCoroutine(FirePrimaryCR());
                        }

                        break;
                    case 1:
                        if (readyToFire && currentAmmo > 0)
                        {
                            readyToFire = false;
                            currentAmmo--;
                            currentAmmo = Mathf.Clamp(currentAmmo, 0, 999);

                            if (m_BarrelShaker != null)
                                m_BarrelShaker.StartShake();

                            _bulletBuffer = Realtime.Instantiate(SecondaryWeaponProjectile.name,
                                position: _barrelTip.position,
                                rotation: _barrelTip.rotation,
                                ownedByClient: true,
                                preventOwnershipTakeover: true,
                                destroyWhenOwnerOrLastClientLeaves: true,
                                useInstance: _realtime);

                            WeaponProjectileBase SecondaryWeaponBase =
                                _bulletBuffer.GetComponent<WeaponProjectileBase>();

                            if (turretAim.missileTargetTransform != null &&
                                _bulletBuffer.GetComponent<MissileProjectile>() != null)
                            {
                                _bulletBuffer.GetComponent<MissileProjectile>()
                                    .SetTarget(turretAim.missileTargetTransform);
                            }

                            turretAim.EmptyTarget();

                            SecondaryWeaponBase.Fire(_barrelTip, ProjectileVelocity(CarRB.velocity));
                            SecondaryWeaponBase.truckDamageTempModifier = tempTruckDamageModifier;
                            SecondaryWeaponBase.statEntity = _player.statsEntity;
                            StartCoroutine(FireSecondaryCR());
                        }
                        else if (currentAmmo <= 0)
                        {
                            if (savedWeaponProjectile != null)
                            {
                                SwitchBackToSavedWeapon();
                            }
                            else
                            {
                                //Do nothing, no ammo!
                                Debug.Log("No ammo remains!");
                            }
                        }

                        break;
                }
            }

            if (readyToFire)
            {
                heatLevel -= 2f;
                heatLevel = Mathf.Clamp(heatLevel, 0, maxHeatThreshold);
            }

            OverheatMeter.fillAmount = (heatLevel / maxHeatThreshold);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (SecondaryWeaponProjectile != null)
            {
                weaponType++;
                weaponType %= 2;
                WeaponSwitcherUI.gameObject.SetActive(true);
                StartCoroutine(CheckSwitchUI());

                switch (weaponType)
                {
                    case 0:
                        WeaponProjectileBase PrimaryWeaponBase =
                            PrimaryWeaponProjectile.GetComponent<WeaponProjectileBase>();
                        uIManager.SwitchProjectileDisplayInfo(PrimaryWeaponBase.ProjectileToDisplay, 999);
                        _barrelTip.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        SwapWeaponMesh(weaponType);
                        break;
                    case 1:
                        WeaponProjectileBase SecondaryWeaponBase =
                            SecondaryWeaponProjectile.GetComponent<WeaponProjectileBase>();
                        _barrelTip.transform.localRotation =
                            Quaternion.Euler(0 - SecondaryWeaponBase.barrelFireAngle, 0, 0);
                        uIManager.SwitchProjectileDisplayInfo(SecondaryWeaponBase.ProjectileToDisplay,
                            (int) currentAmmo);
                        SwapWeaponMesh(weaponType);
                        break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            CycleSyncPhysicsParamsData();
        }

        if ((Input.GetKeyDown(KeyCode.V) || Input.GetMouseButtonDown(1) && turretAim.targetList.Count > 0))
        {
            if (!turretAim.isManualTargeting)
            {
                turretAim.ResetManualTargeting();
            }

            if (!turretAim.isRotating)
            {
                turretAim.CrossHairAnimation();
            }

            turretAim.CycleSelectTarget();
        }

        //Need to add reset timer to avoid spamming
        if (Input.GetKeyDown(KeyCode.R)) //reset
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
                for (int i = 0; i < boostParticles.Length; i++)
                {
                    boostSound.Play();
                    boostParticles[i].Play();
                    foreach (var particle in boostParticles[i].transform.GetComponentsInChildren<ParticleSystem>())
                    {
                        particle.Play();
                    }
                }

                _player.ChangeIsBoosting(true);
                StartCoroutine(StopBoostEffect());
                CarRB.AddForce(transform.forward * (dashForce), ForceMode.VelocityChange);
            }
        }

        /*if (Input.GetKeyDown(KeyCode.U)) //|| Input.GetButtonDown("Lights")) //lights
        {
            lights = !lights;
            RHL.enabled = lights;
            LHL.enabled = lights;
        }*/

        if (GameManager.instance.isHost)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                //PlayerManager
                PlayerManager.instance.SpawnItems();
            }
        }

        //AutoDamage Debug
        //TO REMOVE in testing and final builds
        if (Input.GetKeyDown(KeyCode.O))
        {
            _player.DamagePlayer(5f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            _player.HealPlayer(5f);
        }
    }

    IEnumerator StopBoostEffect()
    {
        yield return new WaitForSeconds(.5f);
        for (int i = 0; i < boostParticles.Length; i++)
        {
            boostParticles[i].Stop();
            foreach (var particle in boostParticles[i].transform.GetComponentsInChildren<ParticleSystem>())
            {
                particle.Stop();
            }
        }

        _player.ChangeIsBoosting(false);
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit ground, GroundCheckRayLength,
            groundLayer);
        //Debug.DrawLine(transform.position, ground.point, Color.cyan);

        Physics.Raycast(transform.position, -transform.up, out RaycastHit rotationAlignment,
            (GroundCheckRayLength + 2f), groundLayer);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            (Quaternion.FromToRotation(transform.up, rotationAlignment.normal)
             * transform.rotation), Time.deltaTime * lerpRotationSpeed);
    }


    void SwapWeaponMesh(float weapon)
    {
        ItemDataProcessor dataProcess = this.GetComponent<ItemDataProcessor>();

        switch (weapon)
        {
            case 0:
                dataProcess.ProcessVisualIndices(new Vector3((dataProcess.WeaponSelectorCount() - 1),
                    lootManager.VisualIndex.y, lootManager.VisualIndex.z));
                ObtainCorrectShaker();
                break;
            case 1:
                dataProcess.ProcessVisualIndices(lootManager.VisualModelIndex());
                ObtainCorrectShaker();
                break;
        }
    }

    void LerpFallCorrection()
    {
        if (!isGrounded)
        {
            this.transform.rotation = Quaternion.Slerp(transform.rotation,
                (Quaternion.FromToRotation(transform.up, Vector3.up)
                 * transform.rotation), Time.deltaTime * lerpRotationSpeed * 0.3f);
        }
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
                float StationaryRotation =
                    (1 + tempSpeedModifier) * turnInput * turnSpd * Time.deltaTime;
                CarRB.AddForce(transform.forward * turningFwdSpeed, ForceMode.Acceleration);
                transform.Rotate(0, StationaryRotation, 0, Space.World);
            }
            else
            {
                float motionRotation = ((1 + tempSpeedModifier) * turnInput * turnSpd *
                                        Time.deltaTime *
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
    private IEnumerator FirePrimaryCR()
    {
        _bombs++;
        //Add Heat Value
        heatLevel += 10f;
        CheckHeatLevels();
        muzzleFlash.SetActive(true);
        StartCoroutine(MuzzleToggle());
        yield return primaryWait;
        //if (WeaponSwitcherUI.activeInHierarchy)
        {
            WeaponSwitcherUI.SetActive(false);
        }
        readyToFire = true;
    }

    void CheckHeatLevels()
    {
        if (heatLevel >= maxHeatThreshold &&
            !Overheat)
        {
            OverHeatNotice.gameObject.SetActive(true);
            Overheat = true;
            StartCoroutine(WeaponCoolDown());
        }
    }

    IEnumerator WeaponCoolDown()
    {
        yield return new WaitForSeconds(OverheatCoolTimer);
        OverHeatNotice.gameObject.SetActive(false);
        heatLevel = 0;
        Overheat = false;
    }

    private IEnumerator FireSecondaryCR()
    {
        _bombs++;
        muzzleFlash.SetActive(true);
        StartCoroutine(MuzzleToggle());
        yield return secondaryWait;
        //if (WeaponSwitcherUI.activeInHierarchy)
        {
            WeaponSwitcherUI.SetActive(false);
        }
        readyToFire = true;
    }

    IEnumerator MuzzleToggle()
    {
        yield return muzzleWait;
        muzzleFlash.SetActive(false);
    }

    private void OnTriggerEnter(Collider collision)
    {
        LootContainer lootbox = collision.gameObject.GetComponent<LootContainer>();
        if (lootbox != null)
        {
            StartCoroutine(lootbox.CR_MeshDie());
            if (_realtimeView.isOwnedLocallyInHierarchy)
            {
                int LootRoll = lootbox.id;
                if (lootbox.collectedBy < 0)
                {
                    lootbox.transform.GetComponent<RealtimeView>().RequestOwnership();
                    lootbox.GetCollected(_realtimeView.ownerIDInHierarchy);
                    if (LootRoll < 0)
                    {
                        //Player got powerup here
                        //Use a decode or script obj to determine what   each temp powerup should be
                        ApplyPowerUpToPlayer(lootManager.DecodePowerUp(LootRoll));
                    }
                }
            }
        }
    }

    public void ApplyPowerUpToPlayer(TempItemSObj PowerUp)
    {
        switch (PowerUp.powerUpType)
        {
            case PowerUpType.Ammo:
                //Idea add weapon ammo pick up modifier
                currentAmmo += ammoEfficiency;
                break;
            case PowerUpType.Boost:
                //Custom boost condition here to do
                tempBoostModifier = PowerUp.PrimaryModifierValue;
                break;
            case PowerUpType.Defense:
                tempDefenseModifier = PowerUp.PrimaryModifierValue;
                _player.UpdateTempDefenseModifier(tempDefenseModifier);
                break;
            case PowerUpType.Health:
                //Heals are per float value based on health
                //Could use percentage
                _player.HealPlayer(PowerUp.PrimaryModifierValue);
                break;
            case PowerUpType.Speed:
                tempSpeedModifier = PowerUp.PrimaryModifierValue;
                break;
            case PowerUpType.SuperGun:
                //Set super gun Projectile Here
                SwitchWeaponsDuringGame
                    (PowerUp.projectileType, PowerUp.PrimaryModifierValue, PowerUp.ExtraWeaponModifierValue);
                break;
            case PowerUpType.TruckAttack:
                tempTruckDamageModifier = PowerUp.PrimaryModifierValue;
                break;
            default:
                Debug.Log("PowerUp Type Not recognized, Check Power Up ID");
                break;
        }
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