using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Normal.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class TurretAutoAim : MonoBehaviour
{
    //For lerping crosshair mostly
    [SerializeField] private GameObject enemy, lastEnemy;

    //For keeping track of currently focused target and to reinitialize autotargeting
    [SerializeField] private Collider currentTarget;
    public GameObject turretFOV;
    public float turretRotationSpd;

    public float maxAngle;
    public float turretDetectionRange;
    public float radarSweepTimer;

    private int targetIndex = 0;

    private Quaternion targetRotation;
    private Quaternion LookAtRotation;

    private NewCarController carController;

    [SerializeField] private Collider[] turretTargets;
    [SerializeField] private LayerMask turretDetectionLayer;

    public List<Collider> targetList = new List<Collider>();

    [SerializeField] private List<Collider> targetsToIgnore = new List<Collider>();

    public bool isManualTargeting = false;
    public bool isRotating = false;

    public bool isSwitchingMode = false;

    private bool isPlayerControlled = true;

    private UIManager m_uiManager;

    public Image CrossHairUI;
    public Camera currentCam;

    [SerializeField] RectTransform parentCanvas;

    public float lerpTime = 1f;
    public float currentLerpTime;

    [SerializeField] private Collider truck;

    Coroutine weaponChange;

    public Transform missileTargetTransform;

    private Truck Truck;
    private NewCarController Player;

    private bool delayTargeting = false;

    int targetlayer = (1 << 12 | 1 << 15 | 1 << 9);

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, turretDetectionRange);
    }

    private void Start()
    {
        targetList.Clear();
        carController = GetComponentInParent<NewCarController>();

        if (carController._realtimeView.isOwnedLocallyInHierarchy)
        {
            m_uiManager = FindObjectOfType<UIManager>();
            CrossHairUI = m_uiManager.CrossHairUI;
            currentCam = m_uiManager.UIcamera;
            parentCanvas = m_uiManager.ScreenCanvas.GetComponent<RectTransform>();
            StartCoroutine(DelayRadarAtStart());
            CrossHairUI.gameObject.SetActive(true);
        }
    }

    private IEnumerator DelayRadarAtStart()
    {
        yield return new WaitForSeconds(2f);
        CycleSelectTarget();
        StartCoroutine(turretRadarSweep());
    }

    void Update()
    {
        if (carController._realtimeView.isOwnedLocallyInHierarchy)
        {
            if (!isSwitchingMode)
            {
                RotateTurret();
                MoveCrossHair();

                if (isPlayerControlled)
                    RotateTurretToMouse();
            }

            //if (Input.GetKey(KeyCode.B))
            //{
            //    if (weaponChange == null)
            //    {
            //        isSwitchingMode = true;
            //        weaponChange = StartCoroutine(ChangeWeaponMode());
            //    }
            //}
        }
    }

    private IEnumerator ChangeWeaponMode()
    {
        //Add weapon Change UI here
        yield return new WaitForSeconds(2f);
        isPlayerControlled = !isPlayerControlled;

        isSwitchingMode = false;
        weaponChange = null;
        //Disable weapon Change UI here
    }

    void MoveCrossHair()
    {
        if (isPlayerControlled)
        {
            if (CrossHairUI.gameObject.activeInHierarchy)
            {
                Vector3 MousePos = Input.mousePosition;
                Vector3 MousePosCalculated = new Vector3(parentCanvas.rect.width * (MousePos.x - 0.5f),
                    parentCanvas.rect.height * (MousePos.y - 0.5f), 0);

                CrossHairUI.rectTransform.anchoredPosition =
                    (Input.mousePosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            }
        }
        else if (enemy != null)
        {
            Vector3 Pos = Camera.main.WorldToViewportPoint(enemy.transform.position);
            Vector3 PosCalculated = new Vector3(parentCanvas.rect.width * (Pos.x - 0.5f),
                parentCanvas.rect.height * (Pos.y - 0.5f), 0);

            if (lastEnemy != null)
            {
                Vector3 LastPos = Camera.main.WorldToViewportPoint(lastEnemy.transform.position);
                Vector3 LastPosCalculated = new Vector3(parentCanvas.rect.width * (LastPos.x - 0.5f),
                    parentCanvas.rect.height * (LastPos.y - 0.5f), 0);


                //CrossHairUI.rectTransform.localPosition = new Vector3(parentCanvas.rect.width * (Pos.x - 0.5f),
                //    parentCanvas.rect.height * (Pos.y - 0.5f), 0);
                currentLerpTime += Time.deltaTime * 3f;

                if (currentLerpTime > lerpTime)
                {
                    currentLerpTime = lerpTime;
                }

                //lerp!
                float perc = currentLerpTime / lerpTime;

                CrossHairUI.rectTransform.anchoredPosition =
                    Vector3.Lerp(LastPosCalculated, PosCalculated, perc);


                if (isRotating)
                {
                    CrossHairUI.rectTransform.localScale = (Vector3.one * 4f);
                    float turningSpd = Mathf.Lerp(1, 5, perc);
                    CrossHairUI.rectTransform.eulerAngles += (new Vector3(0, 0, turningSpd));
                    CrossHairUI.rectTransform.localScale = Vector3.Lerp(new Vector3(
                            CrossHairUI.rectTransform.localScale.x,
                            CrossHairUI.rectTransform.localScale.y, CrossHairUI.rectTransform.localScale.z),
                        Vector3.one,
                        perc);
                }
            }
        }
    }

    public void ResetManualTargeting()
    {
        isManualTargeting = true;
        StartCoroutine(ResetManualTargetingCR());
    }

    private IEnumerator ResetManualTargetingCR()
    {
        yield return new WaitForSeconds(3f);
        isManualTargeting = false;
    }

    public void CrossHairAnimation()
    {
        isRotating = true;
        StartCoroutine(CrossHairAnimationCR());
    }

    private IEnumerator CrossHairAnimationCR()
    {
        yield return new WaitForSeconds(lerpTime);
        isRotating = false;
    }

    private IEnumerator turretRadarSweep()
    {
        while (true)
        {
            if (!isPlayerControlled)
            {
                ObtainTargets();
                RemoveTargets();
            }

            yield return new WaitForSeconds(radarSweepTimer);
        }
    }

    void ObtainTargets()
    {
        turretTargets = Physics.OverlapSphere(transform.position,
            turretDetectionRange, turretDetectionLayer);

        for (int i = 0; i < turretTargets.Length; i++)
        {
            //An id check here would mean removing or ignoring colliders of the same id is not necessary
            if (!targetList.Contains(turretTargets[i]))
            {
                targetList.Add(turretTargets[i]);

                if (turretTargets[i].GetComponent<Truck>())
                {
                    truck = turretTargets[i].GetComponent<Truck>().gameObject.GetComponent<Collider>();
                }

                if (turretTargets[i].GetComponent<NewCarController>() != null)
                {
                    if (turretTargets[i].GetComponent<RealtimeView>().ownerIDInHierarchy ==
                        carController._realtimeView.ownerIDInHierarchy)
                    {
                        targetList.Remove(turretTargets[i]);
                    }
                }
            }
        }
    }

    void RemoveTargets()
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            if (!turretTargets.Contains(targetList[i]))
            {
                if (targetList[i] == truck)
                {
                    truck = null;
                }

                targetList.Remove(targetList[i]);
            }
        }

        //if the sphere cast does not conain the current target
        //remove it and replace it with null
        //This will reinitiatlize auto targeting
        if (!turretTargets.Contains(currentTarget))
        {
            targetList.Remove(currentTarget);
            currentTarget = null;
        }
    }

    public void CycleSelectTarget()
    {
        //Rotate CrossHair code here
        if (targetList.Count != 0 && !isPlayerControlled)
        {
            if (targetIndex < targetList.Count)
            {
                Collider crossHairTargReference = targetList[targetIndex];
                lastEnemy = crossHairTargReference.gameObject;
            }

            targetIndex++;
            targetIndex %= targetList.Count;
            //Debug.Log("Selecting Target " + targetIndex);
            //Collider toREmove = targetList[targetIndex];

            enemy = targetList[targetIndex].gameObject;

            //This ensures autotargeting stays on target as 
            //It selects the first elements in the target list
            //This will make the players gun stay on target and not switch
            //Automatically to another enemy if already locked on

            if (enemy.GetComponent<Collider>() != null)
                currentTarget = enemy.GetComponent<Collider>();

            if (lastEnemy != enemy)
            {
                currentLerpTime = 0;
            }
        }
        else
        {
            Debug.Log("No Targets in Range");
            return;
        }
    }

    void AutoSelectTarget()
    {
        if (targetList.Count == 0)
        {
            Debug.Log("No Targets in Range");
            return;
        }
        else
        {
            if (truck != null)
            {
                enemy = truck.gameObject;
            }
            else
            {
                enemy = targetList[0].gameObject;
            }

            currentTarget = enemy.GetComponent<Collider>();
            lastEnemy = enemy;

            if (!isRotating)
            {
                currentLerpTime = 0;
                CrossHairAnimation();
            }
        }
    }

    void RotateTurret()
    {
        if (!isPlayerControlled)
        {
            if (EnemyInFieldOfView(turretFOV))
            {
                Vector3 direction = enemy.transform.position - transform.position;
                targetRotation = Quaternion.LookRotation(direction);
                LookAtRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    Time.deltaTime * turretRotationSpd);
                transform.rotation = LookAtRotation;
            }
            else
            {
                targetRotation = Quaternion.Euler(0, 0, 0);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation,
                    targetRotation, Time.deltaTime * turretRotationSpd);
            }
        }
    }

    public void EmptyTarget()
    {
        missileTargetTransform = null;
        Truck = null;
        Player = null;
        delayTargeting = true;
        //Prevents next missile from immediately locking on to the previous target
        StartCoroutine(DelayTargetAcquisition());
    }

    private IEnumerator DelayTargetAcquisition()
    {
        yield return new WaitForSeconds(1f);
        delayTargeting = false;
    }

    void RotateTurretToMouse()
    {
        //if (MouseInFieldOfView(turretFOV))
        {
            Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, out hitInfo, Mathf.Infinity, targetlayer))
            {
                if (hitInfo.transform.root.gameObject.name != this.transform.root.gameObject.name)
                {
                    CrossHairUI.gameObject.SetActive(true);
                    Debug.Log("Collided with " + hitInfo.collider.name);

                    Truck = hitInfo.collider.gameObject.GetComponent<Truck>();
                    Player = hitInfo.collider.gameObject.GetComponent<NewCarController>();

                    if (Truck != null && missileTargetTransform == null && !delayTargeting)
                    {
                        missileTargetTransform = Truck.transform;
                    }

                    if (Player != null && missileTargetTransform == null && !delayTargeting)
                    {
                        missileTargetTransform = Player.transform;
                    }

                    //Vector3 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    Vector3 direction = (hitInfo.point - this.transform.position);
                    targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
                else
                {
                    return;
                }

                //Debug.Log("hit " + hitInfo.collider.name);
            }
            else
            {
                CrossHairUI.gameObject.SetActive(false);
            }
        }
    }

    bool EnemyInFieldOfView(GameObject turret)
    {
        if (enemy != null)
        {
            float range = Vector3.Distance(transform.position, enemy.transform.position);
            Vector3 targetDir = enemy.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, turret.transform.forward);

            //If an enemy target is visible and in range of the 
            //cone of detection then set crosshair active to true
            if (angle < maxAngle && range < turretDetectionRange)
            {
                if (!CrossHairUI.gameObject.activeInHierarchy)
                {
                    CrossHairUI.gameObject.SetActive(true);
                }

                return true;
            }
            else
            {
                CrossHairUI.gameObject.SetActive(false);

                return false;
            }
        }
        else
        {
            CrossHairUI.gameObject.SetActive(false);
            return false;
        }
    }
}