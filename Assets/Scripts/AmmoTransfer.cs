using Normal.Realtime;
using UnityEngine;

public class AmmoTransfer : MonoBehaviour
{
    private NewCarController contact;
    private RealtimeView rt => GetComponent<RealtimeView>();

    private void OnTriggerEnter(Collider other)
    {
        contact = other.transform.GetComponent<NewCarController>();
        if (contact == null) return;
        if (contact._realtimeView.ownerIDInHierarchy == rt.ownerIDInHierarchy) return;
        if (contact._realtimeView.isOwnedLocallyInHierarchy)
        {
            contact.currentAmmo++;
            rt.SetOwnership(contact._realtimeView.ownerIDInHierarchy);
            Realtime.Destroy(gameObject);
        }
    }
}