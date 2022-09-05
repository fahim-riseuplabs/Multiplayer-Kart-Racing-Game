using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public float anitRoll = 5000f;
    
    public WheelCollider wheelLF;
    public WheelCollider wheelRF;
    public WheelCollider wheelLB;
    public WheelCollider WheelRB;

    public GameObject COM;

    private Rigidbody RB;
    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.centerOfMass = COM.transform.localPosition;
    }

    public void GroundWheels(WheelCollider WL, WheelCollider WR)
    {
        WheelHit hit;

        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = WL.GetGroundHit(out hit);
        if (groundedL)
        {
            travelL = (-WL.transform.InverseTransformPoint(hit.point).y - WL.radius) / WL.suspensionDistance;                
        }

        bool groundedR = WL.GetGroundHit(out hit);
        if (groundedR)
        {
            travelR = (-WR.transform.InverseTransformPoint(hit.point).y - WR.radius) / WR.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * anitRoll;

        if (groundedL)
        {
            RB.AddForceAtPosition(WL.transform.up * -antiRollForce, WL.transform.position);
        }

        if (groundedR)
        {
            RB.AddForceAtPosition(WR.transform.up * antiRollForce, WR.transform.position);
        }
    }

    void FixedUpdate()
    {
        GroundWheels(wheelLF, wheelRF);
        GroundWheels(wheelLB, WheelRB);
        
    }
}
