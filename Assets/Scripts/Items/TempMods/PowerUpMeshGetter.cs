using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpMeshGetter : MonoBehaviour
{
    LootManager lootManager;

    Loot LootID;

    // Start is called before the first frame update
    void Start()
    {
        lootManager = FindObjectOfType<LootManager>();
        LootID = GetComponent<Loot>();
        ApplyLootMesh(Mathf.Abs(LootID.id));
    }

    public void ApplyLootMesh(int PUIndex)
    {
        PUIndex--;
        if (PUIndex < 0) PUIndex = 0;
        GameObject MeshToApply = lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex].MeshAppearance;

        if (MeshToApply != null)
        {
            this.transform.GetChild(1).gameObject.SetActive(false);
            Instantiate(lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex].MeshAppearance,
                this.gameObject.transform);
        }
    }
}