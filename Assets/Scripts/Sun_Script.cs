using UnityEngine;

public class Sun_Script : MonoBehaviour
{
    public float rotationSpeed;
    public float distanceFromMapCenter;

    private void Awake()
    {
        transform.position = new Vector3(0,distanceFromMapCenter,0);
    }
    private void FixedUpdate()
    {
        transform.RotateAround(Vector3.zero,Vector3.forward,rotationSpeed*Time.deltaTime);
        transform.LookAt(Vector3.zero);
    }
}
