using UnityEngine;
using Normal.Realtime;

public class Pointer : MonoBehaviour
{
    public Transform _master, _target;
    public float blinkSpeed = 1f; //higher=faster
    public float distanceFromMaster = 2f;
    public float yOffset;

    bool _isInitialized;
    Renderer _renderer;
    Material _mat;
    float _emission;
    Color _baseColor, _finalColor;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material;
        _baseColor = Color.red;
    }
    void Update()
    {
        if (_isInitialized)
        {
            if (_target == null || _master == null)
            {
                PlayerManager.instance.RemovePointer(transform);
                Realtime.Destroy(gameObject);
            }
            AnimateRenderer();
            PositionPointer();
        }
    }

    public bool Initialize(Transform master, Transform target)
    {
        bool _test = true;
        if (master != null)
            _master = master;
        else
            _test = false;

        if (target != null)
            _target = target;
        else
            _test = false;
        _isInitialized = _test;

        //Debug.LogWarning("Init: " + _test + " | _master: " + _master + " | _target: " + _target);
        return _test;
    }

    private void PositionPointer()
    {
        if (transform!=null&&_master!=null)
        {
            transform.position =
            _master.position +
            (_target.position - _master.position).normalized *
            distanceFromMaster;
            transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        }        
    }

    private void AnimateRenderer()
    {
        _emission = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
        _finalColor = _baseColor * Mathf.LinearToGammaSpace(_emission);
        _mat.SetColor("_EmissionColor", _finalColor);
    }
}
