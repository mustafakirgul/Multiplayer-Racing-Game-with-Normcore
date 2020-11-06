using Normal.Realtime;
using System.Collections;
using UnityEngine;

public class Bullet : RealtimeComponent<ProjectileModel>
{
    public float startSpeed;
    public float damage;
    public float explosiveRange;
    Rigidbody rb;
    GameObject explosion;
    WaitForSeconds wait1Sec;
    Collider[] colliders;
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
        colliders = new Collider[0];
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
    public void Fire(Transform _barrelTip, float _tipVelocity)
    {
        rb = GetComponent<Rigidbody>();
        explosion = transform.GetChild(0).gameObject;
        explosion.SetActive(false);
        wait1Sec = new WaitForSeconds(1f);
        if (isNetworkInstance)
        {
            rb.isKinematic = true;
            return;
        }
        GetComponent<MeshRenderer>().enabled = true;
        transform.position = _barrelTip.position;
        transform.rotation = _barrelTip.rotation;
        rb.AddForce(
            transform.forward * (startSpeed + _tipVelocity),
            ForceMode.VelocityChange);
    }
    void Hit()
    {
        if (!isNetworkInstance)
        {
            _model.exploded = true;
            StartCoroutine(HitCR());
        }
    }
    IEnumerator HitCR()
    {
        if (!isNetworkInstance)
        {
            rb.isKinematic = true;
        }
        GetComponent<TrailRenderer>().emitting = false;
        colliders = Physics.OverlapSphere(transform.position, explosiveRange);
        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Debug.Log("In Explosion Range:" + colliders[i]);
                    if (colliders[i].gameObject.GetComponent<Player>() != null)
                    {
                        colliders[i].gameObject.GetComponent<Player>().ChangeExplosionForce(colliders[i].transform.position-transform.position);
                        colliders[i].gameObject.GetComponent<Player>().DamagePlayer(damage);
                    }

                }
            }
        }
        if (explosion==null)
        {
            explosion = transform.GetChild(0).gameObject;
        }
        explosion.SetActive(true);
        if (wait1Sec==null)
        {
            wait1Sec = new WaitForSeconds(1f);
        }
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
