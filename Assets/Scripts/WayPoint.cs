using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public int index=-1;

    private void Awake()
    {
        if (index < 0)
            Debug.LogWarning("Waypoint not initialized!");
        GetComponent<MeshRenderer>().enabled = false;
    }
}
