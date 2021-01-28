using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private Rigidbody rb => GetComponent<Rigidbody>();

    [SerializeField]
    private Transform StartPos, EndPos;

    public float speed;

    public bool MoveToTarget, ResetToStart;

    private void Start()
    {
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
            MoveWall(EndPos);
        }

        if (ResetToStart)
        {
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
