using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerSelector : MonoBehaviour
{
    public List<GameObject> ModelToActiavte = new List<GameObject>();
    // Start is called before the first frame update
    private void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!ModelToActiavte.Contains(this.transform.GetChild(i).gameObject))
            {
                ModelToActiavte.Add(this.transform.GetChild(i).gameObject);
            }
        }
    }
   
    public void DeActivateChildren()
    {
        for (int i = 0; i < ModelToActiavte.Count; i++)
        {
            ModelToActiavte[i].SetActive(false);
        }
    }

    public void ActivateItem(int itemIndexToActivate)
    {
        DeActivateChildren();
        ModelToActiavte[itemIndexToActivate].SetActive(true);
    }
}
