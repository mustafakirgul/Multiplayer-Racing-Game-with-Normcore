using UnityEngine;

public class CarPhysicsSphereCleaner : MonoBehaviour
{
    Transform master;
    private void Awake()
    {
        if (master==null)
        {
            master = transform.parent;
        }
    }
    private void Update()
    {
        if (master==null)
        {
            Destroy(gameObject);
        }
    }
}
