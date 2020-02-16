using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{

    // SteerLock is the maximum angle that the wheels may be turned
    [SerializeField] private float fSteerLock = 30.0f;
    // SteerReturn is the degrees per second that the wheels return to 0 degrees deflection at when no steering input is received, low speed
    [SerializeField] private float fSteerReturnLow = 40.0f;
    // SteerReturn is the degrees per second that the wheels return to 0 degrees deflection at when no steering input is received, high speed
    [SerializeField] private float fSteerReturnHigh = 20.0f;
    // SteerAdjust is the degrees per second that the wheels rotate when a steering key is depressed, low speed
    [SerializeField] private float fSteerAdjustLow = 40.0f;
    // SteerAdjust is the degrees per second that the wheels rotate when a steering key is depressed, low speed
    [SerializeField] private float fSteerAdjustHigh = 40.0f;
    // velocity at which steering sensitivity is at a minimum
    [SerializeField] private float fHighVel = 15.0f;
    // enable steer assist
    [SerializeField] private bool bSteerAssist = true;
    // steer assist max slip
    [SerializeField] private float fMaxSlip = 4.5f;


    private float fTimeStep;
    private float fMaxSlipRad;

    void Start()
    {
        fTimeStep = (1.0f / Time.deltaTime);
        fMaxSlipRad = fMaxSlip * Mathf.Deg2Rad;
    }

    public float SteerAngle(float fVel, float fControllerInputX, float fSteerAngle, WheelCollider[] wC)
    {
        float fVelocityScalar = 1.0f - Mathf.Clamp(Mathf.Abs(fVel / fHighVel), 0.0f, 1.0f);
        float fSteerAdjustTotal = (fSteerAdjustHigh + (fSteerAdjustLow - fSteerAdjustHigh) * fVelocityScalar) / fTimeStep;
        float fSteerReturnTotal = (fSteerReturnHigh + (fSteerReturnLow - fSteerReturnHigh) * fVelocityScalar) / fTimeStep;

        float fUpdatedSteerAngle = 0.0f;
        float fSlipLat = 0.0f;
        bool bNoMoreSteer = false;

        if (Mathf.Abs(fControllerInputX) < 0.03f)
        {
            // no steering input, allow the wheels to re-centre
            if (Mathf.Abs(fSteerAngle) > fSteerReturnTotal)
            {
                fUpdatedSteerAngle = fSteerAngle - fSteerReturnTotal * Mathf.Sign(fSteerAngle);
            }
            else fUpdatedSteerAngle = 0.0f;
        }
        else
        {
            // steering requested, adjust steer angle at the defined rate

            // check to see if allowable angular slip is being exceeded to prevent additional steering
            for (int i = 2; i < 4; i++)
            {
                wC[i].GetGroundHit(out WheelHit whContactPatch);
                fSlipLat = whContactPatch.sidewaysSlip;
                if (Mathf.Abs(fSlipLat) > fMaxSlipRad) bNoMoreSteer = true;
            }

            // calculate new proposed steering angle
            fUpdatedSteerAngle = fSteerAngle + fSteerAdjustTotal * Mathf.Sign(fControllerInputX);
            // check it does not exceed steering lock angle, if it does restore to max lock angle
            if (Mathf.Abs(fUpdatedSteerAngle) > fSteerLock) fUpdatedSteerAngle = fSteerLock * Mathf.Sign(fControllerInputX);
            // check angular slip is not already at the steering assist limit, if it is restore to previous angle
            if (bNoMoreSteer && bSteerAssist && (Mathf.Sign(fSlipLat) * Mathf.Sign(fControllerInputX) < 0)) fUpdatedSteerAngle = fSteerAngle;
        }

        return fUpdatedSteerAngle;
    }


    // calculate Ackermann steering angle for selected wheel
    public float AckerAdjusted(float fSteerAngle, float fWheelBase, float fTrackFront, bool bLeft)
    {
        if (fSteerAngle == 0.0f) return 0.0f;
        float fTurnRad = fWheelBase / Mathf.Tan(Mathf.Deg2Rad * fSteerAngle);
        if (bLeft) return Mathf.Rad2Deg * Mathf.Atan(fWheelBase / (fTurnRad + fTrackFront));
        else return Mathf.Rad2Deg * Mathf.Atan(fWheelBase / (fTurnRad - fTrackFront));
    }

}
