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
    public Melee opponent;
    public ParticleSystem crashParticle;
    public NewCarController controller, opponentController;
    public Rigidbody carRB, rb;
    public WaitForSeconds wait;
    public RealtimeView rt;
    public StatsEntity statsEntity;
    public Player player;
    public Realtime realtime => FindObjectOfType<Realtime>();
    public float testMeleeForce = 666f;
    public float LootThrowForce = 100f;

    private void Start()
    {
        rt = GetComponent<RealtimeView>();
        originalOwnerID = rt.ownerIDInHierarchy;
        if (rt.isOwnedRemotelyInHierarchy)
        {
            foreach (var c in FindObjectsOfType<NewCarController>())
            {
                if (c._realtimeView.ownerIDInHierarchy == originalOwnerID)
                {
                    Setup(c);
                }
            }
        }
    }

    public void Setup(NewCarController _controller)
    {
        parent = _controller.transform;
        controller = _controller;
        player = _controller._player;
        statsEntity = player.statsEntity;
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

    private IEnumerator OnTriggerEnter(Collider other)
    {
        opponent = other.transform.GetComponentInChildren<Melee>();
        if (opponent != null)
            opponentController = opponent.controller;
        else
        {
            Debug.LogWarning("No opponent for controller adoption!");
            yield break;
        }

        Debug.DrawLine(transform.position, opponent.transform.position, Color.white);
        Debug.DrawLine(transform.position, transform.forward, Color.green);
        Debug.DrawLine(opponent.transform.position, opponent.transform.forward, Color.red);
        if (controller.isBoosting)
        {
            crashParticle.Play();
            Debug.LogWarning("Melee hit sent by " + PlayerManager.instance.PlayerName(rt.ownerIDInHierarchy));
            yield return wait;
            crashParticle.Stop();
        }
        else if (opponentController.isBoosting)
        {
            GetMeleeHit();
            Debug.LogWarning("Melee hit received by " + PlayerManager.instance.PlayerName(rt.ownerIDInHierarchy));
        }
    }

    private void GetMeleeHit()
    {
        if (statsEntity == null) statsEntity = player.statsEntity;
        carRB.AddForce((parent.position - opponent.transform.position) * testMeleeForce);
        if (statsEntity._loot > 0)
        {
            statsEntity.LoseLoot();
            opponentController._player.statsEntity.ReceiveStat(StatType.loot);
        }
        else
        {
            //give ammo TODO
        }
    }
}