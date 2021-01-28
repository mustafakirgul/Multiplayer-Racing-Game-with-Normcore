using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Wall : MonoBehaviour
{
    private Rigidbody rb => GetComponent<Rigidbody>();

    public Transform StartPos, EndPos;

    public float speed;

    public bool MoveToTarget, ResetToStart;

    private void Start()
    {
        Debug.Log("Wall is owned by: " + GetComponent<RealtimeTransform>().ownerIDInHierarchy);

        if (EndPos != null)
        {
            EndPos.transform.parent = null;
        }

        if (StartPos != null)
        {
            StartPos.transform.parent = null;
        }
    }

    public void GoDown()
    {
        MoveToTarget = true;
    }

    public void GoUp()
    {
        ResetToStart = true;
    }

    private void Update()
    {
        //Two bools to avoid wasting update cycles
        if (MoveToTarget)
        {
            ResetToStart = false;
            MoveWall(EndPos);
        }

        if (ResetToStart)
        {
            MoveToTarget = false;
            MoveWall(StartPos);
        }
    }

    private void MoveWall(Transform target)
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        if (Vector3.Distance(transform.position, target.position) <= 1f)
        {
            MoveToTarget = false;
            ResetToStart = false;
        }
    }
}