using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurretAutoAim : MonoBehaviour
{
    //For lerping crosshair mostly
    [SerializeField]
    private GameObject enemy, lastEnemy;

    //For keeping track of currently focused target and to reinitialize autotargeting
    [SerializeField]
    private Collider currentTarget;
    public GameObject turretFOV;
    public float turretRotationSpd;

    public float maxAngle;
    public float turretDetectionRange;
    public float radarSweepTimer;

    private int targetIndex = 0;

    private Quaternion targetRotation;
    private Quaternion LookAtRotation;

    [SerializeField]
    private float playerColliderID;

    private NewCarController carController;

    [SerializeField]
    private Collider[] turretTargets;
    [SerializeField]
    private LayerMask turretDetectionLayer;

    public List<Collider> targetList = new List<Collider>();

    [SerializeField]
    private List<Collider> targetsToIgnore = new List<Collider>();

    public bool isManualTargeting = false;

    private UIManager m_uiManager;

    public Image CrossHairUI;
    public Camera currentCam;

    [SerializeField]
    RectTransform parentCanvas;

    float lerpTime = 1f;
    public float currentLerpTime;

    private bool isRotating = false;

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, turretDetectionRange);
    }
    private void Start()
    {
        targetList.Clear();
        carController = GetComponentInParent<NewCarController>();

        if (!carController.isNetworkInstance)
        {
            m_uiManager = FindObjectOfType<UIManager>();
            CrossHairUI = m_uiManager.CrossHairUI;
            currentCam = m_uiManager.UIcamera;
            parentCanvas = m_uiManager.ScreenCanvas.GetComponent<RectTransform>();
            StartCoroutine(DelayRadarAtStart());
        }
        playerColliderID = carController.ownerID;
    }

    private IEnumerator DelayRadarAtStart()
    {
        yield return new WaitForSeconds(2f);
        CycleSelectTarget();
        StartCoroutine(turretRadarSweep());
    }
    void Update()
    {
        if (!carController.isNetworkInstance)
        {
            RotateTurret();
            MoveCrossHair();
        }
    }

    void MoveCrossHair()
    {
        if (enemy != null)
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
                    float turningSpd = Mathf.Lerp(1, 5, perc);
                    CrossHairUI.rectTransform.eulerAngles += (new Vector3(0, 0, turningSpd));
                }
            }
        }
    }

    public IEnumerator ResetManualTargetingCR()
    {
        isManualTargeting = true;
        isRotating = true;
        yield return new WaitForSeconds(3f);
        isRotating = false;
        isManualTargeting = false;
    }
    private IEnumerator turretRadarSweep()
    {
        while (true)
        {
            ObtainTargets();
            RemoveTargets();

            //If there are no current targets in range
            //and the player isn't actively trying to target
            //a valid target, initialize autotargeting

            if (!isManualTargeting
                && currentTarget == null)
            {
                AutoSelectTarget();
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

                if (turretTargets[i].GetComponent<NewCarController>() != null)
                {
                    if (turretTargets[i].GetComponent<NewCarController>().ownerID == playerColliderID)
                    { targetList.Remove(turretTargets[i]); }
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
        if (targetList.Count != 0)
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
            { currentLerpTime = 0; }
        }
        else
        {
            Debug.Log("No Targets in Range");
            return;
        }
    }

    void RotateCrossHair(float perc)
    {

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
            enemy = targetList[0].gameObject;
        }
    }

    void RotateTurret()
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
