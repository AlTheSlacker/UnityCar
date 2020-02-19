using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brakes : MonoBehaviour
{

    [SerializeField] private float brakeMaxDeceleration = 0.9f;
    [SerializeField] private float brakeFrontBias = 0.6f;

    private float maxBrakeTorqueFront;
    private float maxBrakeTorqueRear;

    void Start()
    {
        // calculate front/rear max brake force / torques per wheel
        // note brakeFrontBias needs to be modified depending on CoG Z (weight transfer during braking)
        Rigidbody rB = GetComponent<Rigidbody>();
        Suspension suspension = GetComponent<Suspension>();
        float maxBrakeForce = brakeMaxDeceleration * Physics.gravity.y * rB.mass;
        float maxBrakeForceFront = maxBrakeForce * brakeFrontBias / 2.0f;
        float maxBrakeForceRear = maxBrakeForce * (1.0f - brakeFrontBias) / 2.0f;
        maxBrakeTorqueFront = maxBrakeForceFront * suspension.GetRollingRadiusFront;
        maxBrakeTorqueRear = maxBrakeForceRear * suspension.GetRollingRadiusRear;
    }

    public float[] GetBrakeTorques(float inputY)
    {
        float[] brakeTorques = new float[4];
        for (int i = 0; i < 4; i++)
        {
            if (inputY >= 0.05f) brakeTorques[i] = 0.0f;
            else
            {
                if (i < 2) brakeTorques[i] = maxBrakeTorqueRear * inputY;
                else brakeTorques[i] = maxBrakeTorqueFront * inputY;
            }
        }
        return brakeTorques;
    }

    public float[] ApplyHandbrake(float[] brakeTorques)
    {
        for (int i = 0; i < 2; i++)
        {
            brakeTorques[i] = maxBrakeTorqueRear * -2.0f;
        }
        return brakeTorques;
    }
}
