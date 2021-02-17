using UnityEngine;

public class PodiumController : MonoBehaviour
{
    public float defaultRotationSpeed = 1;
    [SerializeField] private bool isRotating;
    Transform child => transform.GetChild(0);
    float rotation;

    // Start is called before the first frame update
    public void StartRotation()
    {
        isRotating = true;
    }

    public void StopRotation()
    {
        isRotating = false;
    }

    private void Update()
    {
        if (isRotating)
        {
            rotation = (rotation + defaultRotationSpeed * Time.deltaTime) % 360f;
            child.localEulerAngles = new Vector3(
                0,
                rotation,
                0);
        }
    }
}