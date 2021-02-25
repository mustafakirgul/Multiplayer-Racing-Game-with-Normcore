using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDecoder : MonoBehaviour
{
    LootManager lootManager;

    public List<GameObject> LootDecoderUnits = new List<GameObject>();
    public int indexToActivate;
    public int childCount;

    public bool canCheck;

    public GameObject LootDecoderUnitToSpawn;
    private void Start()
    {
        lootManager = FindObjectOfType<LootManager>();
        LootDecoderUnits.Clear();
        canCheck = false;
    }

    public float CheckForChildren()
    {
        childCount = this.transform.childCount;

        return childCount;
    }

    public void StartSequence()
    {
        if (childCount > 0)
        {
            //Initiate activation sequence for each loot
            this.transform.GetChild(0).gameObject.SetActive(true);
            indexToActivate = 0;
        }
    }

    public void Update()
    {
        if (canCheck && Input.GetKeyDown(KeyCode.Space))
        {
            TryNextSequenceInLine();
        }
    }

    public void ActivateNextInSequence()
    {
        this.transform.GetChild(indexToActivate).gameObject.SetActive(false);

        indexToActivate++;

        if (indexToActivate <= childCount)
        {
            this.transform.GetChild(indexToActivate).gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Sequence ended for loot rolls");
        }
    }

    public void TryNextSequenceInLine()
    {
        float animTime = this.transform.GetChild(indexToActivate).gameObject.GetComponent<Animator>().
            GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (animTime >= 1)
        {
            ActivateNextInSequence();
        }
        else
        {
            Debug.Log("Animation not ended playing");
        }
    }
}
