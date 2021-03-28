using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField]
    private List<Transform> LocationMarker = new List<Transform>();
    [SerializeField]
    PlayerManager playerManager;
    [SerializeField]
    float speed;
    [SerializeField]
    bool isMoving = false;
    [SerializeField]
    int MoveIndex = 0;
    [SerializeField]
    Transform target = null;

    [SerializeField]
    float MarkDistance;

    [SerializeField]
    float lerpValue;
    public void StartMoving()
    {
        playerManager = FindObjectOfType<PlayerManager>();

        //if (playerManager.localPlayer.transform != null)
        //{
        //    LocationMarker.Add(playerManager.localPlayer.transform);
        //}

        target = LocationMarker[0].transform;
        MarkDistance = Vector3.Distance(this.transform.position, target.position);
        isMoving = true;
    }
    void Update()
    {
        if (isMoving)
        {
            Moving();
            CheckDistance();
        }
    }
    void Moving()
    {
        float step = speed * Time.deltaTime; // calculate distance to move
        this.transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        lerpValue = (1 - Vector3.Distance(transform.position, target.position) / MarkDistance);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, target.localRotation, lerpValue);
    }
    void CheckDistance()
    {
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            MoveIndex++;
            CheckForFinalPoint();
        }
    }
    void CheckForFinalPoint()
    {
        if (MoveIndex < LocationMarker.Count)
        {
            target = LocationMarker[MoveIndex].transform;
            MarkDistance = Vector3.Distance(this.transform.position, target.position);
        }
        else
        {
            if (LocationMarker.Contains(playerManager.localPlayer.transform))
            {
                LocationMarker.Remove(playerManager.localPlayer.transform);
            }

            StartCoroutine(WaitToExit());
        }
    }

    private IEnumerator WaitToExit()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}
