using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Wall : MonoBehaviour
{
    private Rigidbody rb => GetComponent<Rigidbody>();

    public float speed, resetSpeed;

    public bool MoveToTarget, ResetToStart;

    Vector3 startingPos = new Vector3();

    float step;

    private void Start()
    {
        Debug.Log("Wall is owned by: " + GetComponent<RealtimeTransform>().ownerIDInHierarchy);

        startingPos = new Vector3
        (this.transform.position.x,
        this.transform.position.y,
        this.transform.position.z);
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
            MoveWall();
        }

        if (ResetToStart)
        {
            startingPos =
                new Vector3
        (this.transform.position.x,
        this.transform.position.y,
        this.transform.position.z);

            MoveToTarget = false;
            MoveWall();
        }
    }

    private void MoveWall()
    {
        if (MoveToTarget)
        {
            step = -speed * Time.deltaTime;
        }
        else
        {
            step = resetSpeed * Time.deltaTime;
        }

        transform.position += Vector3.up * 1f * step;

        if (Vector3.Distance(startingPos, transform.position) >= 150f)
        {
            MoveToTarget = false;
            ResetToStart = false;
        }
    }
}