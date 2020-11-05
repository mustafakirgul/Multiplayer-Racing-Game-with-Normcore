using Normal.Realtime;
using System.Collections;
using UnityEngine;

public class Bullet : RealtimeComponent<ProjectileModel>
{
    public float firePower;
    public float damage;
    Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait1Sec;
    RaycastHit[] hits;
    private ProjectileModel _model;
    public bool isNetworkInstance = true;
    RealtimeView _realtimeView;
    RealtimeTransform _realtimeTransform;


    protected override void OnRealtimeModelReplaced(ProjectileModel previousModel, ProjectileModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.explodedDidChange -= UpdateExplosionState;
        }
        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            // use [ if (currentModel.isFreshModel)] to initialize player prefab
            _model = currentModel;
            currentModel.explodedDidChange += UpdateExplosionState;
        }
    }
    void KillTimer()
    {
        Hit();
    }
    void UpdateExplosionState(ProjectileModel model, bool _state)
    {
        if (_state && isNetworkInstance)
        {
            StartCoroutine(HitCR());
        }
    }
    private void Awake()
    {
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
    }

    private void Start()
    {
        if (_realtimeView.isOwnedLocallySelf)
        {
            isNetworkInstance = false;
            Invoke(nameof(KillTimer), 20f);
        }
        _model.exploded = false;
    }

    private void Update()
    {
        if (isNetworkInstance || _model == null)
            return;
        _realtimeView.RequestOwnership();
        _realtimeTransform.RequestOwnership();
    }
    private void LateUpdate()
    {
        if (isNetworkInstance)
            return;
        if (transform.position.y < -300)
        {
            Realtime.Destroy(gameObject);
        }
    }
    public void Fire(Transform _barrelTip)
    {
        if (isNetworkInstance)
            return;
        rb = GetComponent<Rigidbody>();
        explosion = transform.GetChild(0).gameObject;
        wait1Sec = new WaitForSeconds(1f);
        hits = new RaycastHit[0];
        GetComponent<MeshRenderer>().enabled = true;
        explosion.SetActive(false);
        transform.position = _barrelTip.position;
        transform.rotation = _barrelTip.rotation;
        rb.AddForce(
            transform.forward * firePower,
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
                    hits[i].transform.gameObject.GetComponent<Rigidbody>().AddExplosionForce(damage, transform.position, 30f);
                }
                Debug.Log(hits[i].transform.name);
            }
        }
        if (!isNetworkInstance)
        {
            _model.exploded = true;
            StartCoroutine(HitCR());
        }
    }
    IEnumerator HitCR()
    {
        rb.isKinematic = true;
        GetComponent<TrailRenderer>().emitting = false;
        explosion.SetActive(true);
        GetComponent<MeshRenderer>().enabled = false;
        yield return wait1Sec;
        yield return wait1Sec;
        explosion.SetActive(false);
        yield return wait1Sec;
        Realtime.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isNetworkInstance)
            return;
        Hit();
    }
}
