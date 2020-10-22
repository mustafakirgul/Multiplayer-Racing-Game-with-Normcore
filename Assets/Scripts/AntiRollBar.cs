using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public Rigidbody carBody;
    public WheelCollider WheelL;
    public WheelCollider WheelR;
    public float AntiRoll = 5000f;
    private WheelHit hit;

    void FixedUpdate()
    {
        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = WheelL.GetGroundHit(out hit);

        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        bool groundedR = WheelR.GetGroundHit(out hit);

        if (groundedR)
            travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            carBody.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
                   WheelL.transform.position);
        if (groundedR)
            carBody.AddForceAtPosition(WheelR.transform.up * antiRollForce,
                   WheelR.transform.position);
    }
}
