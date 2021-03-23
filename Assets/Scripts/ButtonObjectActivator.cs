using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonObjectActivator : MonoBehaviour
{
    [SerializeField]
    GameObject ObjectToActivate;
    public void ActivateObj()
    {
        ObjectToActivate.SetActive(true);
    }

    public void DisableObj()
    {
        ObjectToActivate.SetActive(false);
    }
}
