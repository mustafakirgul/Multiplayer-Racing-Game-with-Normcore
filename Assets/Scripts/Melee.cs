using System;
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
    private Rigidbody carRB;

    private void Start()
    {
        isNetworkInstance = GetComponent<RealtimeView>().isOwnedLocallyInHierarchy;
        controller = GetComponent<NewCarController>();
        carRB = controller.CarRB;
    }

    public Vector3 ReturnVelocity()
    {
        return carRB.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isNetworkInstance) return;
        opponent = collision.transform.GetComponent<Melee>();
        if (opponent == null) return;
        contactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(contactPoints);
        CalculateCollision(opponent, contactPoints);
    }

    private void CalculateCollision(Melee melee, ContactPoint[] _contactPoints)
    {
        //calculate middlepoint for collision
        Vector3 total = Vector3.zero;
        Vector3 collisionCenter = Vector3.zero;
        Vector3 calculatedForce = Vector3.zero;
        int count = _contactPoints.Length;

        for (int i = 0; i < count; i++)
        {
            total = new Vector3(
                total.x + _contactPoints[i].point.x,
                total.y + _contactPoints[i].point.y,
                total.z + _contactPoints[i].point.z
            );
        }

        collisionCenter = new Vector3(
            total.x / count,
            total.y / count,
            total.z / count
        );

        NetworkedCollision calculatedCollision = new NetworkedCollision(calculatedForce, collisionCenter);
    }
}

public struct NetworkedCollision
{
    public NetworkedCollision(Vector3 force, Vector3 point)
    {
        this.force = force;
        this.point = point;
    }

    public NetworkedCollision GetInfo()
    {
        return this;
    }

    private Vector3 force;
    private Vector3 point;
}