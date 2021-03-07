using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshLerper : MonoBehaviour
{
    Player playerHealth;

    [SerializeField]
    MeshRenderer currentRenderer;

    [SerializeField]
    Material[] MatPool;
    [SerializeField]
    Material nextDmgState;

    public float lerpDmgStart, DmgEnd, sectionedHP;

    public int currentDmgIndex;

    void Start()
    {
        playerHealth = GetComponentInParent<Player>();
        currentRenderer = GetComponent<MeshRenderer>();
        ComputerDamageCounters();
        currentDmgIndex = 0;
        ResetMats();
    }

    void Update()
    {
        LerpBasedOnHealth();

        if (playerHealth.playerHealth <= 0)
        {
            ResetMats();
        }
    }
    private void ComputerDamageCounters()
    {
        if (MatPool.Length > 1)
        {
            sectionedHP = playerHealth.maxPlayerHealth / (MatPool.Length - 1);
        }
    }

    private void ResetMats()
    {
        currentDmgIndex = 0;
        nextDmgState = MatPool[1];
        DmgEnd = playerHealth.maxPlayerHealth - sectionedHP;
    }

    private void AssignDamageSegment()
    {
        currentDmgIndex++;
        currentDmgIndex %= (MatPool.Length);
        nextDmgState = MatPool[currentDmgIndex];
        DmgEnd = playerHealth.maxPlayerHealth - (currentDmgIndex * sectionedHP);
    }
    private void LerpBasedOnHealth()
    {
        if (playerHealth.playerHealth <= DmgEnd)
        {
            currentRenderer.material = nextDmgState;

            AssignDamageSegment();
        }
    }
}
