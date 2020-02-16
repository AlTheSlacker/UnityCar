using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{

    [SerializeField] private float fSuspDisplacementRangeFront = 0.30f;
    [SerializeField] private float fSuspDisplacementRangeRear = 0.30f;
    [SerializeField] private float fSuspStiffnessFront = 50000.0f;
    [SerializeField] private float fSuspStiffnessRear = 50000.0f;
    [SerializeField] private float fSuspDamperFront = 1600.0f;
    [SerializeField] private float fSuspDamperRear = 1600.0f;
    [SerializeField] private float fFARBe = 1.0f;
    [SerializeField] private float fRARBe = 1.0f;
    [SerializeField] private float fFrontBumperFAxleDistance = 1.0f;
    [SerializeField] private float fFrontBumperRAxleDistance = 4.0f;
    [SerializeField] private float fTrackFront = 1.6f;
    [SerializeField] private float fTrackRear = 1.6f;
    [SerializeField] private float fRollingRadiusFront = 0.325f;
    [SerializeField] private float fRollingRadiusRear = 0.325f;
    [SerializeField] private float fTyreRadiusFront = 0.335f;
    [SerializeField] private float fTyreRadiusRear = 0.335f;
    [SerializeField] private float fUnsprungMassFront = 20.0f;
    [SerializeField] private float fUnsprungMassRear = 20.0f;

    private float fTargetPositionFront = 0.5f;
    private float fTargetPositionRear = 0.5f;
    private float fWheelBase;
    private float[] faStaticCornerLoad;
    private float fTyreFrontOffset = 0.02f;
    private float fTyreRearOffset = 0.02f;
    private WheelCollider[] wC;

    public float GetWheelBase { get { return fWheelBase; } }
    public float[] GetStaticCornerLoad { get { return faStaticCornerLoad; } }
    public float GetTyreFrontOffset { get { return fTyreFrontOffset; } }
    public float GetTyreRearOffset { get { return fTyreRearOffset; } }
    public float GetRollingRadiusFront { get { return fRollingRadiusFront; } }
    public float GetRollingRadiusRear { get { return fRollingRadiusRear; } }
    public float GetTrackFront { get { return fTrackFront; } }


    void Start()
    {

        Rigidbody rB = GetComponent<Rigidbody>();

        // calculate wheelbase
        fWheelBase = fFrontBumperRAxleDistance - fFrontBumperFAxleDistance;

        // calculate the wheel centre correction for rolling radius flat spot
        fTyreFrontOffset = fTyreRadiusFront - fRollingRadiusFront;
        fTyreRearOffset = fTyreRadiusRear - fRollingRadiusRear;

        // set up front and rear suspension springs
        JointSpring jSpringFront;
        JointSpring jSpringRear;
        jSpringFront.spring = fSuspStiffnessFront;
        jSpringFront.damper = fSuspDamperFront;
        jSpringFront.targetPosition = fTargetPositionFront;
        jSpringRear.spring = fSuspStiffnessRear;
        jSpringRear.damper = fSuspDamperRear;
        jSpringRear.targetPosition = fTargetPositionRear;

        wC = gameObject.GetComponentsInChildren<WheelCollider>();

        // WC sequence RL RR FL FR
        for (int i = 0; i < 4; i++)
        {
            wC[i].ConfigureVehicleSubsteps(30, 8, 20);
            if (i < 2)
            {
                wC[i].radius = fRollingRadiusRear;
                wC[i].mass = fUnsprungMassRear;
                wC[i].center = new Vector3(0.0f, fSuspDisplacementRangeRear * (1.0f - fTargetPositionRear), 0.0f);
                wC[i].suspensionDistance = fSuspDisplacementRangeRear;
                wC[i].suspensionSpring = jSpringRear;
                wC[i].forceAppPointDistance = 0.0f;
                wC[i].wheelDampingRate = 0.0001f;
            }
            else
            {
                wC[i].radius = fRollingRadiusFront;
                wC[i].mass = fUnsprungMassFront;
                wC[i].center = new Vector3(0.0f, fSuspDisplacementRangeFront * (1.0f - fTargetPositionFront), 0.0f);
                wC[i].suspensionDistance = fSuspDisplacementRangeFront;
                wC[i].suspensionSpring = jSpringFront;
                wC[i].forceAppPointDistance = 0.0f;
                wC[i].wheelDampingRate = 0.0001f;
            }
        }

        // position the wheelcolliders according to vehicle dimensions
        // RL RR FL FR
        wC[0].gameObject.transform.localPosition = new Vector3(fTrackRear / -2.0f, fRollingRadiusRear, -fFrontBumperRAxleDistance);
        wC[1].gameObject.transform.localPosition = new Vector3(fTrackRear / 2.0f, fRollingRadiusRear, -fFrontBumperRAxleDistance);
        wC[2].gameObject.transform.localPosition = new Vector3(fTrackFront / -2.0f, fRollingRadiusFront, -fFrontBumperFAxleDistance);
        wC[3].gameObject.transform.localPosition = new Vector3(fTrackFront / 2.0f, fRollingRadiusFront, -fFrontBumperFAxleDistance);

        // calculate static corner loads
        float fMass = 1000.0f;
        faStaticCornerLoad = new float[4];
        fMass = rB.mass;
        // calculate CoG from front axle (care sign convention)
        float fFrontAxleToCoG = (fFrontBumperRAxleDistance - fFrontBumperFAxleDistance) / 2.0f;
        fFrontAxleToCoG = Mathf.Abs(rB.centerOfMass.z) - fFrontBumperFAxleDistance;
        // calcualate corner mass for front and rear
        faStaticCornerLoad[0] = fFrontAxleToCoG / fWheelBase * fMass / 2.0f * Physics.gravity.y;
        faStaticCornerLoad[1] = faStaticCornerLoad[0];
        faStaticCornerLoad[2] = (1 - fFrontAxleToCoG / fWheelBase) * fMass / 2.0f * Physics.gravity.y;
        faStaticCornerLoad[3] = faStaticCornerLoad[2];
    }



    public float GetNoSlipWheelRPM(float fVel)
    {
        // Don't return 0.0f for div0 reasons
        float fNoSlipWheelRPM = fVel / (6.283f * fRollingRadiusRear) * 60.0f;
        if (fNoSlipWheelRPM == 0.0f) fNoSlipWheelRPM = 0.01f;
        return fNoSlipWheelRPM;
    }

    public void ApplyLLT(Rigidbody RB, WheelCollider[] WC)
    {
        // apply anti-rollbar load transfer
        float fTravelL, fTravelR, fARBDisp;
        float fTransferForce;
        float fARBe = fRARBe;
        float fSuspK = fSuspStiffnessRear;
        // RL RR FL FR
        for (int i = 0; i <= 2; i = i + 2)
        {
            if (i == 2)
            {
                fARBe = fFARBe;
                fSuspK = fSuspStiffnessFront;
            }
            fTravelL = WC[i].gameObject.transform.GetChild(0).transform.localPosition.y;
            fTravelR = WC[i + 1].gameObject.transform.GetChild(0).transform.localPosition.y;
            fARBDisp = Mathf.Abs(fTravelL - fTravelR);
            fTransferForce = fARBDisp * fARBe * fSuspK / 2.0f;
            RB.AddForceAtPosition(WC[i].transform.up * fTransferForce * Mathf.Sign(fTravelL), WC[i].transform.position);
            RB.AddForceAtPosition(WC[i + 1].transform.up * fTransferForce * Mathf.Sign(fTravelR), WC[i + 1].transform.position);
        }
    }

    public void RePosition()
    {

        // calculate wheelbase
        fWheelBase = fFrontBumperRAxleDistance - fFrontBumperFAxleDistance;

        // calculate the wheel centre correction for rolling radius flat spot
        fTyreFrontOffset = fTyreRadiusFront - fRollingRadiusFront;
        fTyreRearOffset = fTyreRadiusRear - fRollingRadiusRear;

        // set up front and rear suspension springs
        JointSpring jSpringFront;
        JointSpring jSpringRear;
        jSpringFront.spring = fSuspStiffnessFront;
        jSpringFront.damper = fSuspDamperFront;
        jSpringFront.targetPosition = fTargetPositionFront;
        jSpringRear.spring = fSuspStiffnessRear;
        jSpringRear.damper = fSuspDamperRear;
        jSpringRear.targetPosition = fTargetPositionRear;
    }

}