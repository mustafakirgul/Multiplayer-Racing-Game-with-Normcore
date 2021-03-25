using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDeactivator : MonoBehaviour
{
    [SerializeField]
    GameObject ObjToDeactivate;

    private void OnTriggerEnter(Collider other)
    {
        //if(other.transform.root.GetComponent<NewCarController>() != null)
        {
            ObjToDeactivate.SetActive(false);
        }
    }
}
