using UnityEngine;

public class PoolMember : MonoBehaviour
{
    public int poolID;
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Freeze()
    {
        if (WeaponPool.instance.pools[poolID].activeCopies.Contains(this))
            WeaponPool.instance.pools[poolID].activeCopies.Remove(this);
        if (!WeaponPool.instance.pools[poolID].passiveCopies.Contains(this))
            WeaponPool.instance.pools[poolID].passiveCopies.Add(this);
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        gameObject.SetActive(false);
    }
    public GameObject Thaw()
    {
        if (WeaponPool.instance.pools[poolID].passiveCopies.Contains(this))
            WeaponPool.instance.pools[poolID].passiveCopies.Remove(this);
        if (!WeaponPool.instance.pools[poolID].activeCopies.Contains(this))
            WeaponPool.instance.pools[poolID].activeCopies.Add(this);
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        gameObject.SetActive(true);
        return gameObject;
    }
}
