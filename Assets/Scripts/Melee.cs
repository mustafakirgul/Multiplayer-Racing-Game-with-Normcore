using System.Collections;
using Normal.Realtime;
using UnityEngine;

[RequireComponent(typeof(RealtimeView))]
public class Melee : MonoBehaviour
{
    public int originalOwnerID;
    public float weight;
    public float meleePower;
    public float armorFactor;
    public Transform parent;
    private Melee opponent;
    public ParticleSystem crashParticle;
    private NewCarController controller;
    private Rigidbody carRB, rb;
    private WaitForSeconds wait;
    private RealtimeView rt;

    public void Setup(Transform _parent, NewCarController _controller)
    {
        parent = _parent;
        rt = GetComponent<RealtimeView>();
        rt.RequestOwnership();
        originalOwnerID = rt.ownerIDInHierarchy;
        controller = _controller;
        carRB = controller.CarRB;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        wait = new WaitForSeconds(.25f);
        if (crashParticle == null) crashParticle = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (parent == null || rb == null) return;
        rb.MovePosition(parent.position);
        rb.MoveRotation(parent.rotation);
    }

    public Vector3 ReturnVelocity()
    {
        return carRB.velocity;
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        opponent = collision.transform.GetComponentInChildren<Melee>();
        if (opponent == null) yield break;
        Debug.DrawLine(transform.position, opponent.transform.position, Color.white);
        Debug.DrawLine(transform.position, transform.forward, Color.green);
        Debug.DrawLine(opponent.transform.position, opponent.transform.forward, Color.red);
        if (controller.isBoosting)
        {
            crashParticle.Play();
            Debug.LogWarning("Melee happened!");
            yield return wait;
            crashParticle.Stop();
        }
        else
            Debug.LogWarning("Melee did not happen!");
    }
}