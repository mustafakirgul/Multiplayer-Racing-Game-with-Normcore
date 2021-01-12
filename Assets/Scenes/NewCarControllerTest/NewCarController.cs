using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class NewCarController : MonoBehaviour
{
    [SerializeField]
    Rigidbody CarRB;

    private float moveInput, turnInput;
    public float fwdSpeed, reverseSpd, turnSpd, turningFwdSpeed;
    public float airDrag, groundDrag;
    public float GroundCheckRayLength;

    public LayerMask groundLayer;
    public float lerpRotationSpeed;

    public float MinVelocityThreshold;
    public float BrakeForce;
    public float MaxSpeed;
    public float MaxSpeedModifier;

    [SerializeField]
    private bool isGrounded;
    public float extraGravity;
    [SerializeField]
    public ArcadeWheel[] wheels;
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

    //Neworking Related Functionalities
    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;
    public Realtime _realtime;
    public float ownerID;
    private ChaseCam followCamera;
    [SerializeField]
    private Transform CameraContainer;

    //Weapon Controls
    [SerializeField]
    private GameObject WeaponProjectile;
    private GameObject _bulletBuffer;
    public Transform _barrelTip;
    public float fireRate;//number of bullets fired per second //Weapon Change should affect this variable
    public bool readyToFire = false;
    public GameObject muzzleFlash;
    float fireTimer;
    public int ProjectileOwnerID;

    private WaitForSeconds wait, muzzleWait;

    //QA
    [HideInInspector]
    public int _bombs;
    public bool offlineTest;

    [SerializeField]
    private bool isNetworkInstance = true;

    private void Awake()
    {
        if (!offlineTest)
        {
            _realtime = FindObjectOfType<Realtime>();
            _realtimeView = GetComponent<RealtimeView>();
            _realtimeTransform = GetComponent<RealtimeTransform>();
            ownerID = _realtime.room.clientID;
            fireTimer = 1f / fireRate;
        }
        wait = new WaitForSeconds(fireTimer);
        muzzleWait = new WaitForSeconds(.2f);
    }

    void InitCamera()
    {
        followCamera = GameObject.FindObjectOfType<ChaseCam>();
        followCamera.InitCamera(CameraContainer);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Decouple Sphere Physics from car model
        CarRB.transform.parent = null;
        wheelCount = wheels.Length;
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].originY = wheels[i].wheelT.localPosition.y;
        }

        currentX = carBody.localEulerAngles.x;
        currentZ = carBody.localEulerAngles.z;

        if (_realtimeView.isOwnedLocallySelf)
        {
            isNetworkInstance = false;
            //uIManager = FindObjectOfType<UIManager>();
            //uIManager.EnableUI();
            //speedDisplay = uIManager.speedometer;
            //healthRadialLoader = uIManager.playerHealthRadialLoader;
            //IDDisplay.gameObject.SetActive(false);
            //IDDisplay = uIManager.playerName;
            //boostRadialLoader = uIManager.boostRadialLoader;
            //StartCoroutine(BoostCounter());
            //StartCoroutine(FireCR());
            InitCamera();

            PlayerManager.instance.AddLocalPlayer(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (!offlineTest)
        //{
        //    _realtimeView.RequestOwnership();
        //    _realtimeTransform.RequestOwnership();
        //}

        if (!isNetworkInstance)
        {
            _realtimeView.RequestOwnership();
            _realtimeTransform.RequestOwnership();
            for (int i = 0; i < wheelCount; i++)
            {
                wheels[i].wheelRTV.RequestOwnership();
                wheels[i].wheelRT.RequestOwnership();
            }

            DetectInput();
            DragCheck();
            GroundCheck();
            RotationCheck();
            TurnTheWheels();
            transform.position = CarRB.transform.position;
        }
    }
    private void FixedUpdate()
    {
        if (!isNetworkInstance)
        {
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
            wheels[i].trail.GetComponent<TrailRenderer>().emitting = hit.distance < wheels[i].suspensionHeight + wheels[i].wheelSize;

            Debug.DrawLine(wheels[i].t.position, hit.point, Color.red);

            wheels[i].wheelT.localPosition = new Vector3(wheels[i].wheelT.localPosition.x, wheels[i].originY - _tempSuspensionDistance + wheels[i].wheelSize, wheels[i].wheelT.localPosition.z);

            if (wheels[i].isSteeringWheel)
            {
                _tempQ = wheels[i].t.localRotation;
                wheels[i].wheelT.localEulerAngles = new Vector3(_tempQ.eulerAngles.x, turnInput * maxSteeringAngle, _tempQ.eulerAngles.z);
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

        //Networking for wheels


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
            currentX = Mathf.Clamp(Mathf.LerpAngle(currentX, -moveInput * maxXRotation, xRotationLERPSpeed * Time.deltaTime), -XFactor, XFactor);

        }

        if (ZTimer > 0f)
        {
            ZTimer -= Time.deltaTime;
            ZFactor = (ZTimer / rotationCooldownTime) * maxZrotation;
            currentZ = Mathf.Clamp(Mathf.LerpAngle(currentZ, turnInput * maxZrotation, zRotationLERPSpeed * Time.deltaTime), -ZFactor, ZFactor);
        }


        carBody.localEulerAngles = new Vector3(currentX, carBody.localEulerAngles.y, currentZ);


        if (moveInput > 0)
        {
            moveInput *= fwdSpeed;
        }
        else
        {
            moveInput *= reverseSpd;
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
                _bulletBuffer.GetComponent<WeaponProjectileBase>().ownerID = ProjectileOwnerID;

                StartCoroutine(FireCR());
            }
        }
    }
    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit ground, GroundCheckRayLength, groundLayer);
        Debug.DrawLine(transform.position, ground.point, Color.cyan);

        Physics.Raycast(transform.position, -transform.up, out RaycastHit rotationAlignment, (GroundCheckRayLength + 2f), groundLayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.FromToRotation(transform.up, rotationAlignment.normal) 
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
        //transform.rotation = Quaternion.Euler(transform.rotation.x, newRotation, transform.rotation.z);
        //transform.Rotate(0, Mathf.Lerp(transform.rotation.eulerAngles.y, newRotation, Time.deltaTime), 0, Space.World);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, newRotation, 0), Time.deltaTime);

        //Debug.Log(" Vertical input is: " + moveInput);
        //if only pressing turning there should be a small amount of acceleration/speed so car is not turning on its own without momentum
        if (isGrounded)
        {

            if (turnInput != 0 && moveInput == 0)
            {
                //Debug.Log("Stationary Turning");
                float StationaryRotation = turnInput * turnSpd * Time.deltaTime;
                CarRB.AddForce(transform.forward * turningFwdSpeed, ForceMode.Acceleration);
                transform.Rotate(0, StationaryRotation, 0, Space.World);
            }
            else
            {
                float motionRotation = turnInput * turnSpd * Time.deltaTime * Input.GetAxisRaw("Vertical");
                transform.Rotate(0, motionRotation, 0, Space.World);
            }
        }
    }

    public float LocalVelocity()
    {
        Vector3 localVelocity = CarRB.transform.InverseTransformDirection(CarRB.velocity);
        return localVelocity.z;
    }

    //Framerate aware damping
    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    float CarAngle()
    {
        float carAngle = 0;
        Vector3 tangente = new Vector3();
        RaycastHit hit2;
        if (Physics.Raycast(CarRB.transform.position, -CarRB.transform.up, out hit2, 10, groundLayer))
        {
            hit2.normal.Normalize();
            var distance = -Vector3.Dot(hit2.normal, Vector3.up);
            //Debug.DrawRay(hit2.point, (Vector3.up + hit2.normal * distance).normalized , Color.white);
            tangente = (Vector3.up + hit2.normal * distance).normalized;
            carAngle = Vector3.Angle(tangente, -Vector3.up);                                                        // the current car angle
                                                                                                                    //Debug.DrawRay(hit2.point,  -Vector3.up, Color.cyan);
        }
        return carAngle;
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
    public Transform t;//connection point to the car
    public Transform wheelT;//transform of actual wheel
    public RealtimeView wheelRTV;
    public RealtimeTransform wheelRT;
    public GameObject trail;
    public bool isPowered;
    public bool isSteeringWheel;
    public float suspensionHeight;
    public float wheelSize;//radius of the wheel (r)
    public float originY;
}
