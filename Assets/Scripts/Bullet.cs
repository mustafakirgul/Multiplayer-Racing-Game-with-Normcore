using Normal.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviour
{
    public float firePower;
    public float damage;
    Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait2Secs;
    RaycastHit[] hits;
    private void Awake()
    {
        wait2Secs = new WaitForSeconds(2f);
        rb = GetComponent<Rigidbody>();
        explosion = transform.GetChild(0).gameObject;
        explosion.SetActive(false);
        hits = new RaycastHit[0];
    }

    public void Fire(Transform _barrelTip, float _barrelFactor)
    {

        if (GetComponent<RealtimeView>().isOwnedLocallySelf)
            GetComponent<RealtimeTransform>().RequestOwnership();
        if (transform.GetChild(0).GetComponent<RealtimeView>().isOwnedLocallySelf)
        {
            transform.GetChild(0).GetComponent<RealtimeTransform>().RequestOwnership();
        }
        transform.position = _barrelTip.position;
        transform.rotation = _barrelTip.rotation;
        rb.AddForce(
            transform.forward * firePower + ((transform.forward * firePower) * _barrelFactor),
            ForceMode.VelocityChange);
    }

    void Hit()
    {
        if (Physics.SphereCastNonAlloc(transform.position, 10f, transform.up, hits, 200, Physics.AllLayers) > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject.GetComponent<Rigidbody>() != null)
                {
                    hits[i].transform.gameObject.GetComponent<Rigidbody>().AddExplosionForce(damage, transform.position, 10f);
                }
            }
        }
        StartCoroutine(HitCR());
    }
    IEnumerator HitCR()
    {
        rb.isKinematic = true;
        explosion.SetActive(true);
        yield return wait2Secs;
        explosion.SetActive(false);
        GetComponent<PoolMember>().Freeze();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Hit();
    }
}
