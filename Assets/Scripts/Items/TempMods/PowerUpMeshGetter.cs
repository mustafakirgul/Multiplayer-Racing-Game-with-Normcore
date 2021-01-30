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
        GameObject MeshToApply = lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex - 1].MeshAppearance;

        if (MeshToApply != null)
        {
            this.transform.GetChild(1).gameObject.SetActive(false);
            Instantiate(lootManager.playerLootPoolSave.PlayerPowerUps[PUIndex - 1].MeshAppearance, this.gameObject.transform);
        }
    }
}
