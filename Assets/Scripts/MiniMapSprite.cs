using UnityEngine;

public class MiniMapSprite : MonoBehaviour
{
    Transform _parent;
    Vector3 _origin;
    void Start()
    {
        _parent = transform.parent;
        _origin = transform.rotation.eulerAngles;
    }

    void Update()
    {
        if (transform != null && _parent != null)
        {
            transform.rotation = Quaternion.Euler(
                _origin.x,
                _parent.rotation.eulerAngles.y,
                _origin.z);
        }
    }
}
