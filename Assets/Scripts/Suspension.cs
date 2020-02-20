using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{

    [SerializeField] private float suspDisplacementRangeFront = 0.30f;
    [SerializeField] private float suspDisplacementRangeRear = 0.30f;
    [SerializeField] private float suspStiffnessFront = 25000.0f;
    [SerializeField] private float suspStiffnessRear = 25000.0f;
    [SerializeField] private float suspDamperFront = 1200.0f;
    [SerializeField] private float suspDamperRear = 1200.0f;
    [SerializeField] private float fARBe = 0.2f;
    [SerializeField] private float rARBe = 0.2f;
    [SerializeField] private float frontBumperFAxleDistance = 1.0f;
    [SerializeField] private float frontBumperRAxleDistance = 4.0f;
    [SerializeField] private float trackFront = 1.6f;
    [SerializeField] private float trackRear = 1.6f;
    [SerializeField] private float rollingRadiusFront = 0.335f;
    [SerializeField] private float rollingRadiusRear = 0.335f;
    [SerializeField] private float unsprungMassFront = 20.0f;
    [SerializeField] private float unsprungMassRear = 20.0f;

    private float targetPositionFront = 0.5f;
    private float targetPositionRear = 0.5f;
    private float wheelBase;
    private float[] staticCornerLoad;
    private WheelCollider[] wC;
    private Rigidbody rB;

    public float GetWheelBase { get { return wheelBase; } }
    public float[] GetStaticCornerLoad { get { return staticCornerLoad; } }
    public float GetRollingRadiusFront { get { return rollingRadiusFront; } }
    public float GetRollingRadiusRear { get { return rollingRadiusRear; } }
    public float GetTrackFront { get { return trackFront; } }


    void Start()
    {


        // calculate wheelbase
        wheelBase = frontBumperRAxleDistance - frontBumperFAxleDistance;
        rB = GetComponent<Rigidbody>();

        // set up front and rear suspension springs
        JointSpring springFront;
        JointSpring springRear;
        springFront.spring = suspStiffnessFront;
        springFront.damper = suspDamperFront;
        springFront.targetPosition = targetPositionFront;
        springRear.spring = suspStiffnessRear;
        springRear.damper = suspDamperRear;
        springRear.targetPosition = targetPositionRear;

        wC = gameObject.GetComponentsInChildren<WheelCollider>();

        // WC sequence RL RR FL FR
        for (int i = 0; i < 4; i++)
        {
            wC[i].ConfigureVehicleSubsteps(30, 8, 20);
            if (i < 2)
            {
                wC[i].radius = rollingRadiusRear;
                wC[i].mass = unsprungMassRear;
                wC[i].center = new Vector3(0.0f, suspDisplacementRangeRear * (1.0f - targetPositionRear), 0.0f);
                wC[i].suspensionDistance = suspDisplacementRangeRear;
                wC[i].suspensionSpring = springRear;
                wC[i].forceAppPointDistance = 0.0f;
                wC[i].wheelDampingRate = 0.0001f;
            }
            else
            {
                wC[i].radius = rollingRadiusFront;
                wC[i].mass = unsprungMassFront;
                wC[i].center = new Vector3(0.0f, suspDisplacementRangeFront * (1.0f - targetPositionFront), 0.0f);
                wC[i].suspensionDistance = suspDisplacementRangeFront;
                wC[i].suspensionSpring = springFront;
                wC[i].forceAppPointDistance = 0.0f;
                wC[i].wheelDampingRate = 0.0001f;
            }
        }

        // position the wheelcolliders according to vehicle dimensions
        // RL RR FL FR
        wC[0].gameObject.transform.localPosition = new Vector3(trackRear / -2.0f, rollingRadiusRear, -frontBumperRAxleDistance);
        wC[1].gameObject.transform.localPosition = new Vector3(trackRear / 2.0f, rollingRadiusRear, -frontBumperRAxleDistance);
        wC[2].gameObject.transform.localPosition = new Vector3(trackFront / -2.0f, rollingRadiusFront, -frontBumperFAxleDistance);
        wC[3].gameObject.transform.localPosition = new Vector3(trackFront / 2.0f, rollingRadiusFront, -frontBumperFAxleDistance);

        // calculate static corner loads
        staticCornerLoad = new float[4];
        // calculate CoG from front axle (care sign convention)
        float frontAxleToCoG = (frontBumperRAxleDistance - frontBumperFAxleDistance) / 2.0f;
        frontAxleToCoG = Mathf.Abs(rB.centerOfMass.z) - frontBumperFAxleDistance;
        // calcualate corner mass for front and rear
        staticCornerLoad[0] = frontAxleToCoG / wheelBase * rB.mass / 2.0f * Physics.gravity.y;
        staticCornerLoad[1] = staticCornerLoad[0];
        staticCornerLoad[2] = (1 - frontAxleToCoG / wheelBase) * rB.mass / 2.0f * Physics.gravity.y;
        staticCornerLoad[3] = staticCornerLoad[2];
    }

    public float GetNoSlipWheelRPM(float vel)
    {
        // Don't return 0.0f for div0 reasons
        float noSlipWheelRPM = vel / (6.283f * rollingRadiusRear) * 60.0f;
        if (noSlipWheelRPM == 0.0f) noSlipWheelRPM = 0.01f;
        return noSlipWheelRPM;
    }

    public void ApplyLLT()
    {
        // apply anti-rollbar load transfer
        float travelL, travelR, aRBDisp;
        float transferForce;
        float aRBe = rARBe;
        float suspK = suspStiffnessRear;
        // RL RR FL FR
        for (int i = 0; i <= 2; i = i + 2)
        {
            if (i == 2)
            {
                aRBe = fARBe;
                suspK = suspStiffnessFront;
            }
            travelL = wC[i].gameObject.transform.GetChild(0).transform.localPosition.y;
            travelR = wC[i + 1].gameObject.transform.GetChild(0).transform.localPosition.y;
            aRBDisp = Mathf.Abs(travelL - travelR);
            transferForce = aRBDisp * aRBe * suspK / 2.0f;
            rB.AddForceAtPosition(wC[i].transform.up * transferForce * Mathf.Sign(travelL), wC[i].transform.position);
            rB.AddForceAtPosition(wC[i + 1].transform.up * transferForce * Mathf.Sign(travelR), wC[i + 1].transform.position);
        }
    }
}