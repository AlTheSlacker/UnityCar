using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCar
{

    public class Steering : MonoBehaviour
    {

        // SteerLock is the maximum angle that the wheels may be turned
        [SerializeField] private float steerLock = 30.0f;
        // SteerReturn is the degrees per second that the wheels return to 0 degrees deflection at when no steering input is received, low speed
        [SerializeField] private float steerReturnLow = 40.0f;
        // SteerReturn is the degrees per second that the wheels return to 0 degrees deflection at when no steering input is received, high speed
        [SerializeField] private float steerReturnHigh = 20.0f;
        // SteerAdjust is the degrees per second that the wheels rotate when a steering key is depressed, low speed
        [SerializeField] private float steerAdjustLow = 40.0f;
        // SteerAdjust is the degrees per second that the wheels rotate when a steering key is depressed, low speed
        [SerializeField] private float steerAdjustHigh = 40.0f;
        // velocity at which steering sensitivity is at a minimum
        [SerializeField] private float highVel = 15.0f;
        // enable steer assist
        [SerializeField] private bool steerAssist = true;
        // steer assist max slip
        [SerializeField] private float maxSlip = 4.5f;


        private float timeStep;
        private float maxSlipRad;
        private WheelCollider[] wC;

        void Start()
        {
            wC = gameObject.GetComponentsInChildren<WheelCollider>();
            timeStep = (1.0f / Time.deltaTime);
            maxSlipRad = maxSlip * Mathf.Deg2Rad;
        }

        public float SteerAngle(float vel, float controllerInputX, float steerAngle)
        {
            float velocityScalar = 1.0f - Mathf.Clamp(Mathf.Abs(vel / highVel), 0.0f, 1.0f);
            float steerAdjustTotal = (steerAdjustHigh + (steerAdjustLow - steerAdjustHigh) * velocityScalar) / timeStep;
            float steerReturnTotal = (steerReturnHigh + (steerReturnLow - steerReturnHigh) * velocityScalar) / timeStep;

            float updatedSteerAngle;
            float slipLat = 0.0f;
            bool noMoreSteer = false;

            if (Mathf.Abs(controllerInputX) < 0.03f)
            {
                // no steering input, allow the wheels to re-centre
                if (Mathf.Abs(steerAngle) > steerReturnTotal)
                {
                    updatedSteerAngle = steerAngle - steerReturnTotal * Mathf.Sign(steerAngle);
                }
                else updatedSteerAngle = 0.0f;
            }
            else
            {
                // steering requested, adjust steer angle at the defined rate

                // check to see if allowable angular slip is being exceeded to prevent additional steering
                for (int i = 2; i < 4; i++)
                {
                    wC[i].GetGroundHit(out WheelHit contactPatch);
                    slipLat = contactPatch.sidewaysSlip;
                    if (Mathf.Abs(slipLat) > maxSlipRad) noMoreSteer = true;
                }

                // calculate new proposed steering angle
                updatedSteerAngle = steerAngle + steerAdjustTotal * Mathf.Sign(controllerInputX);
                // check it does not exceed steering lock angle, if it does restore to max lock angle
                if (Mathf.Abs(updatedSteerAngle) > steerLock) updatedSteerAngle = steerLock * Mathf.Sign(controllerInputX);
                // check angular slip is not already at the steering assist limit, if it is restore to previous angle
                if (noMoreSteer && steerAssist && (Mathf.Sign(slipLat) * Mathf.Sign(controllerInputX) < 0)) updatedSteerAngle = steerAngle;
            }

            return updatedSteerAngle;
        }


        // calculate Ackermann steering angle for selected wheel
        public float AckerAdjusted(float steerAngle, float wheelBase, float trackFront, bool left)
        {
            if (steerAngle == 0.0f) return 0.0f;
            float turnRad = wheelBase / Mathf.Tan(Mathf.Deg2Rad * steerAngle);
            if (left) return Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRad + trackFront));
            else return Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRad - trackFront));
        }

    }
}