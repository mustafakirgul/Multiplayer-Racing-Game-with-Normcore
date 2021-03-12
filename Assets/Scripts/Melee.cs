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
    private Melee opponent;
    private ContactPoint[] contactPoints;
    public ParticleSystem crashParticle;
    private NewCarController controller;
    private Rigidbody carRB, rb;
    private WaitForSeconds wait;
    private RealtimeView rt;

    private void Start()
    {
        rt = GetComponent<RealtimeView>();
        rt.RequestOwnership();
        originalOwnerID = rt.ownerIDInHierarchy;
        controller = transform.parent.GetComponent<NewCarController>();
        carRB = controller.CarRB;
        wait = new WaitForSeconds(.25f);
        if (crashParticle == null) crashParticle = GetComponentInChildren<ParticleSystem>();
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