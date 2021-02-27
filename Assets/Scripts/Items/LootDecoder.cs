using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDecoder : MonoBehaviour
{
    [SerializeField]
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
        for (int i = 0; i < CheckForChildren(); i++)
        {
            GameObject toDestroy = this.transform.GetChild(i).gameObject;
            LootDecoderUnits.Remove(toDestroy);
            Destroy(toDestroy);
        }

        foreach (GameObject promptToAdd in LootDecoderUnits)
        {
            promptToAdd.transform.parent = this.gameObject.transform;

            //Normalize position and scale

            promptToAdd.SetActive(false);
        }

        if (CheckForChildren() > 0)
        {
            //Reset transform and scale (some weird thing about canvas makes the scale off)
            LootDecoderUnits[0].GetComponent<RectTransform>().localPosition = Vector3.zero;
            LootDecoderUnits[0].GetComponent<RectTransform>().localScale = Vector3.one;

            LootDecoderUnits[0].SetActive(true);
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


        if (indexToActivate < CheckForChildren() - 1)
        {
            indexToActivate++;

            GameObject prompt = this.transform.GetChild(indexToActivate).gameObject;

            //Reset transform and scale (some weird thing about canvas makes the scale off)
            prompt.GetComponent<RectTransform>().localPosition = Vector3.zero;
            prompt.GetComponent<RectTransform>().localScale = Vector3.one;

            prompt.SetActive(true);
        }
        else
        {
            Debug.Log("Sequence ended for loot rolls");
        }
    }

    public void TryNextSequenceInLine()
    {
        if (indexToActivate < CheckForChildren())
        {
            float animTime = this.transform.GetChild(indexToActivate).gameObject.GetComponentInChildren<Animator>().
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
}
