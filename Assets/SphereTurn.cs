using UnityEngine;

public class SphereTurn : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localEulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
    }
}