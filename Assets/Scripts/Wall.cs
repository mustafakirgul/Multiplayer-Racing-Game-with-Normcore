using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.UIElements;

public class Wall : MonoBehaviour
{
    public float speed, resetSpeed, MoveRange;

    public bool MoveToTarget, ResetToStart;

    float step;
    public float localY;

    private void Start()
    {
        localY = 0f;
    }

    public void ResetWall()
    {
        int temp = GetComponent<RealtimeTransform>().ownerIDInHierarchy;
        Debug.Log("Wall is owned by: " + temp);
        MoveToTarget = false;
        ResetToStart = false;
        localY = 0;
        if (GetComponent<RealtimeTransform>().isUnownedInHierarchy)
        {
            transform.localPosition = Vector3.zero;
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
            MoveWall();
        }

        if (ResetToStart)
        {
            MoveToTarget = false;
            MoveWall();
        }

        transform.localPosition = new Vector3(0, localY, 0);
    }

    private void MoveWall()
    {
        if (MoveToTarget)
        {
            step = -speed * Time.deltaTime;
            localY += step;
            if (localY <= -MoveRange)
            {
                MoveToTarget = false;
            }
        }
        else if (ResetToStart)
        {
            step = resetSpeed * Time.deltaTime;
            localY += step;
            if (localY >= 0f)
            {
                ResetToStart = false;
                localY = 0f;
            }
        }
    }
}