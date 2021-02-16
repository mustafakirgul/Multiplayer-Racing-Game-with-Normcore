using System.Collections;
using Normal.Realtime;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public float weight;
    public float meleePower;
    public float armorFactor;
    private Melee opponent;
    private bool isNetworkInstance;
    private ContactPoint[] contactPoints;
    public ParticleSystem crashParticle;
    private NewCarController controller;
    private Rigidbody carRB, rb;
    private WaitForSeconds wait;
    private Transform parent;

    private void Start()
    {
        parent = transform.parent;
        isNetworkInstance = !parent.GetComponent<RealtimeView>().isOwnedLocallyInHierarchy;
        controller = parent.GetComponent<NewCarController>();
        carRB = controller.CarRB;
        rb = GetComponent<Rigidbody>();
        wait = new WaitForSeconds(.25f);
        transform.SetParent(null);
        if (crashParticle == null) crashParticle = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (parent == null) return;

        rb.MovePosition(parent.position);
        rb.MoveRotation(parent.rotation);
    }

    public Vector3 ReturnVelocity()
    {
        return rb.velocity;
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        if (isNetworkInstance) yield break;
        opponent = collision.transform.GetComponentInChildren<Melee>();
        if (opponent == null) yield break;
        Vector3 collisionForce = -(opponent.ReturnVelocity() + ReturnVelocity());
        Debug.DrawLine(transform.position, transform.position + collisionForce, Color.red);
        crashParticle.Play();
        carRB.AddForce(collisionForce * 10000f * meleePower);
        Debug.LogWarning("Melee happened!" + collisionForce);
        yield return wait;
        crashParticle.Stop();
    }
}