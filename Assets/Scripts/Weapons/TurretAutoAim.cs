using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurretAutoAim : MonoBehaviour
{
    [SerializeField]
    private GameObject enemy;
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

    private NewCarController carConroller;

    private Collider[] turretTargets;
    [SerializeField]
    private LayerMask turretDetectionLayer;

    public List<Collider> targetList;

    [SerializeField]
    private List<Collider> targetsToIgnore;

    public bool isManualTargeting = false;

    private UIManager m_uiManager;

    public Image CrossHairUI;
    public Camera currentCam;

    [SerializeField]
    RectTransform parentCanvas;

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, turretDetectionRange);
    }
    private void Start()
    {
        carConroller = GetComponentInParent<NewCarController>();

        if (!carConroller.isNetworkInstance)
        {
            m_uiManager = FindObjectOfType<UIManager>();
            CrossHairUI = m_uiManager.CrossHairUI;
            currentCam = m_uiManager.UIcamera;
            parentCanvas = m_uiManager.ScreenCanvas.GetComponent<RectTransform>();
            StartCoroutine(turretRadarSweep());
            playerColliderID = carConroller.ownerID;
        }
    }
    void Update()
    {
        if (!carConroller.isNetworkInstance)
        {
            RotateTurret();

            if (enemy != null)
            {
                Vector3 Pos = Camera.main.WorldToViewportPoint(enemy.transform.position);
                CrossHairUI.rectTransform.localPosition = new Vector3(parentCanvas.rect.width * (Pos.x - 0.5f),
                    parentCanvas.rect.height * (Pos.y - 0.5f), 0);
            }
        }
    }

    public IEnumerator ResetManualTargetingCR()
    {
        isManualTargeting = true;
        yield return new WaitForSeconds(3f);
        isManualTargeting = false;
    }

    private IEnumerator turretRadarSweep()
    {
        while (true)
        {
            ObtainTargets();
            RemoveTargets();
            if (!isManualTargeting)
                AutoSelectTarget();
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
        if (turretTargets.Length != targetList.Count)
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                if (!turretTargets.Contains(targetList[i]))
                {
                    targetList.Remove(targetList[i]);
                }
            }
        }
    }

    public void CycleSelectTarget()
    {
        if (targetList.Count != 0)
        {
            targetIndex++;
            targetIndex %= targetList.Count;
            Debug.Log("Selecting Target " + targetIndex);
            Collider toREmove = targetList[targetIndex];
            enemy = targetList[targetIndex].gameObject;



            //This ensures autotargeting stays on target as 
            //It selects the first elements in the target list
            targetList.Remove(toREmove);
            targetList.Insert(0, toREmove);
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
