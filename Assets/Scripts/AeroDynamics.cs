using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroDynamics : MonoBehaviour
{

    [SerializeField] private float frontalArea = 0.7f;
    [SerializeField] private float cd = 0.30f;
    [SerializeField] private float clFront = -0.05f;
    [SerializeField] private float clRear = -0.05f;

    private float velDependentLiftFront;
    private float velDependentLiftRear;
    private float velDependentDrag;

    private const float airDensity = 1.292f;


    void Start()
    {
        // Calculate the velocity dependent lift and drag factors
        // fTempDragVar is only used here
        float tempDragVar = airDensity * frontalArea * 0.5f;
        // lift is divided over 4 wheels, it's applied at the wheel locations for simplicity/stability
        // fCl is the coefficient of lift, negative values of fCl = downforce 
        velDependentLiftFront = clFront * tempDragVar / 4.0f;
        velDependentLiftRear = clRear * tempDragVar / 4.0f;
        // fCd is the coefficient of drag, note the sign control as Z+ is car forward
        velDependentDrag = cd * tempDragVar;
    }

    public void ApplyAeroDrag(Rigidbody rb, WheelCollider[] wc, float vel)
    {
        float velSq = vel * vel;
        float drag = velDependentDrag * velSq;
        if (vel < 0.0f) drag = -drag;
        rb.AddRelativeForce(0.0f, 0.0f, -drag, ForceMode.Force);
    }

    public void ApplyAeroLift(Rigidbody rb, WheelCollider[] wc, float vel)
    {
        float velSq = vel * vel;
        float liftFront = velDependentLiftFront * velSq;
        float liftRear = velDependentLiftRear * velSq;
        for (int i = 0; i < 2; i++)
        {
            rb.AddForceAtPosition(wc[i].transform.up * liftRear, wc[i].transform.position);
            rb.AddForceAtPosition(wc[i + 2].transform.up * liftFront, wc[i + 2].transform.position);
        }
    }

}
