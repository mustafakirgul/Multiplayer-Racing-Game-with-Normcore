using UnityEngine;

public class MiniMapSprite : MonoBehaviour
{
    Transform _parent;
    Vector3 _origin;
    void Start()
    {
        _parent = transform.parent;
        transform.parent = null;
        _origin = transform.rotation.eulerAngles;
    }

    void Update()
    {
        transform.position = _parent.position;
        transform.rotation = Quaternion.Euler(_origin.x, _parent.rotation.eulerAngles.y, _origin.z);
    }
}
