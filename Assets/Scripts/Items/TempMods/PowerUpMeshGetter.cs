using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpMeshGetter : MonoBehaviour
{
    LootManager lootManager;

    Loot LootID;

    LootContainer LootContainerID;

    // Start is called before the first frame update
    void Start()
    {
        lootManager = FindObjectOfType<LootManager>();

        StartCoroutine(waitToApplyMesh());
        //ApplyLootMesh(LootID.id);
    }

    private void Update()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + (180 * Time.deltaTime), 0);
    }

    private IEnumerator waitToApplyMesh()
    {
        yield return new WaitForSeconds(0f);
        LootID = GetComponent<Loot>();
        LootContainerID = GetComponent<LootContainer>();
        ApplyLootMesh(Mathf.Abs(LootContainerID.id));
    }

    public void ApplyLootMesh(int PUIndex)
    {
        PUIndex--;

        if (PUIndex < 0)
            PUIndex = 0;

        if (lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex].MeshAppearance != null)
        {
            //Debug.LogWarning("MeshtoApply: " + PUIndex);
            this.transform.GetChild(1).gameObject.SetActive(false);
            Instantiate(lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex].MeshAppearance,
                this.gameObject.transform);
        }
        else
            Debug.LogWarning("No Mesh To Apply");
    }
}