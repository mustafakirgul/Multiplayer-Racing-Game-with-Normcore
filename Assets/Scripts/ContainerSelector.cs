using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerSelector : MonoBehaviour
{
    public List<GameObject> ModelToActivate = new List<GameObject>();
    // Start is called before the first frame update
    private void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!ModelToActivate.Contains(this.transform.GetChild(i).gameObject))
            {
                ModelToActivate.Add(this.transform.GetChild(i).gameObject);
            }
        }
    }
   
    public void DeActivateChildren()
    {
        for (int i = 0; i < ModelToActivate.Count; i++)
        {
            ModelToActivate[i].SetActive(false);
        }
    }

    public int GetChildCount()
    {
        return this.transform.childCount;
    }

    public void ActivateItem(int itemIndexToActivate)
    {
        DeActivateChildren();
        ModelToActivate[itemIndexToActivate].SetActive(true);
    }
}
