using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brakes : MonoBehaviour
{

    [SerializeField] private float fBrakeMaxDeceleration = 0.9f;
    [SerializeField] private float fBrakeFrontBias = 0.6f;

    private float fMaxBrakeTorqueFront;
    private float fMaxBrakeTorqueRear;

    void Start()
    {
        // calculate front/rear max brake force / torques per wheel
        // note fBrakeFrontBias needs to be modified depending on CoG Z (weight transfer during braking)
        Rigidbody rB = GetComponent<Rigidbody>();
        Suspension suspension = GetComponent<Suspension>();
        float fMaxBrakeForce = fBrakeMaxDeceleration * Physics.gravity.y * rB.mass;
        float fMaxBrakeForceFront = fMaxBrakeForce * fBrakeFrontBias / 2.0f;
        float fMaxBrakeForceRear = fMaxBrakeForce * (1.0f - fBrakeFrontBias) / 2.0f;
        fMaxBrakeTorqueFront = fMaxBrakeForceFront * suspension.GetRollingRadiusFront;
        fMaxBrakeTorqueRear = fMaxBrakeForceRear * suspension.GetRollingRadiusRear;
    }

    public float[] GetBrakeTorques(float fInputY)
    {
        float[] faBrakeTorques = new float[4];
        for (int i = 0; i < 4; i++)
        {
            if (fInputY >= 0.05f) faBrakeTorques[i] = 0.0f;
            else
            {
                if (i < 2) faBrakeTorques[i] = fMaxBrakeTorqueRear * fInputY;
                else faBrakeTorques[i] = fMaxBrakeTorqueFront * fInputY;
            }
        }
        return faBrakeTorques;
    }

    public float[] ApplyHandbrake(float[] faBrakeTorques)
    {
        for (int i = 0; i < 2; i++)
        {
            faBrakeTorques[i] = fMaxBrakeTorqueRear * -2.0f;
        }
        return faBrakeTorques;
    }
}
