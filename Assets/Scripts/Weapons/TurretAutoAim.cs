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

    private Collider[] turretTargets;
    [SerializeField]
    private LayerMask turretDetectionLayer;

    [SerializeField]
    private List<Collider> targetList;

    [SerializeField]
    private List<Collider> targetsToIgnore;

    [SerializeField]
    private bool isManualTargeting = false;

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
        StartCoroutine(turretRadarSweep());
        CrossHairUI = GameManager.instance.uIManager.CrossHairUI;
        currentCam = GameManager.instance.uIManager.UIcamera;
        parentCanvas = GameManager.instance.uIManager.ScreenCanvas.GetComponent<RectTransform>();
    }
    void Update()
    {
        RotateTurret();

        if ((Input.GetKeyDown(KeyCode.V)) && targetList.Count > 1)
        {
            CycleSelectTarget();
            if (!isManualTargeting)
            {
                StartCoroutine(ResetManualTargetingCR());
            }
        }

        if (enemy != null && CrossHairUI.gameObject.activeInHierarchy)
        {
            Vector3 Pos = Camera.main.WorldToViewportPoint(enemy.transform.position);
            CrossHairUI.rectTransform.localPosition = new Vector3(Pos.x * parentCanvas.rect.width - parentCanvas.rect.width * 0.5f,
                parentCanvas.rect.height * Pos.y - parentCanvas.rect.height * 0.5f, 0);
        }
    }

    private IEnumerator ResetManualTargetingCR()
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
            if (!targetList.Contains(turretTargets[i]))
            {
                targetList.Add(turretTargets[i]);
            }
        }

        for (int i = 0; i < targetList.Count; i++)
        {
            for (int j = 0; j < targetsToIgnore.Count; j++)
            {
                if (targetList[i] == targetsToIgnore[j])
                {
                    targetList.Remove(targetList[i]);
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

    void CycleSelectTarget()
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
