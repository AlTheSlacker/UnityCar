using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController : MonoBehaviour
{
    [HideInInspector] public ParticleSystem[] Particles;

    [SerializeField] private float mass = 1200.0f;
    [SerializeField] private Vector3 coG = new Vector3(0.0f, 0.435f, -2.5f);
    [SerializeField] private Vector3 inertiaTensor = new Vector3(3600.0f, 3900.0f, 800.0f);

    public float GetVel { get { return vel; } }
    public float GetMass { get { return mass; } }
    public Vector3 GetCoG { get { return coG; } }
    public Rigidbody GetRB { get { return rB; } }

    private WheelCollider[] wC;
    private Rigidbody rB;
    private float vel;
    private float steerAngle = 0.0f;

    private AeroDynamics aeroDynamics;
    private UserInput userInput;
    private Steering steering;
    private Suspension suspension;
    private Brakes brakes;

    void Awake()
    {
        aeroDynamics = GetComponent<AeroDynamics>();
        userInput = GetComponent<UserInput>();
        steering = GetComponent<Steering>();
        suspension = GetComponent<Suspension>();

        // set the physics clock to 120 Hz
        Time.fixedDeltaTime = 0.008333f;
        // Find the wheel colliders and put them in an array
        // Do not re-order the CD_Colliders in the vehicle model, everything depends on them being RL RR FL FR
        // WC sequence RL RR FL FR
        wC = gameObject.GetComponentsInChildren<WheelCollider>();
        // Get and configure the vehicle rigidbody
        rB = GetComponent<Rigidbody>();
        rB.mass = mass;
        rB.centerOfMass = coG;
        rB.inertiaTensor = inertiaTensor;
        rB.isKinematic = false;
    }


    void FixedUpdate()
    {
        // see if there is a controller script active and if so take input from it
        float inputX = 0.0f;
        float inputY = 0.0f;
        float inputR = 0.0f;
        float inputH = 0.0f;
        if (userInput != null)
        {
            if (userInput.enabled)
            {
                inputX = userInput.ControllerInputX;
                inputY = userInput.ControllerInputY;
                inputR = userInput.ControllerInputReverse;
                inputH = userInput.ControllerInputHandBrake;
            }
        }

        // calculate vehicle velocity in the forward direction
        vel = transform.InverseTransformDirection(rB.velocity).z;

        // update aerodynamic drag and lift
        aeroDynamics.ApplyAeroDrag(rB, wC, vel);
        aeroDynamics.ApplyAeroLift(rB, wC, vel);

        

        // update steering angle due to input, correct for ackermann and apply steering (if we have a steering script)
        if (steering != null && suspension != null)
        {
            steerAngle = steering.SteerAngle(vel, inputX, steerAngle, wC);
            wC[2].steerAngle = steering.AckerAdjusted(steerAngle, suspension.GetWheelBase, suspension.GetTrackFront, true);
            wC[3].steerAngle = steering.AckerAdjusted(steerAngle, suspension.GetWheelBase, suspension.GetTrackFront, false);
        }

        /*
        // update lateral load transfer from anti-roll bars (sway bars)
        if (cd_suspension != null) cd_suspension.ApplyLLT(RB, WC);

        // if automatic, select gear appropriate for vehicle speed (unless reverse requested)
        if (cd_transmission != null && cd_suspension != null)
        {
            if (cd_transmission.GetAutomatic)
            {
                if (fInputR > 0.1) cd_transmission.SelectReverse();
                else cd_transmission.SetGear(cd_suspension.GetNoSlipWheelRPM(fVel));
            }
        }

        // update engine rpm and available torque
        float fTransmissionRatio = 1.0f;
        float fEngineClutchLockRPM = 0.0f;
        float fEngineTorque = 0.0f;
        if (cd_engine != null && cd_suspension != null)
        {
            if (cd_transmission != null)
            {
                fTransmissionRatio = cd_transmission.GetTransmissionRatio();
                fEngineClutchLockRPM = cd_transmission.GetEngineClutchLockRPM;
            }
            cd_engine.UpdateEngineSpeedRPM(cd_suspension.GetNoSlipWheelRPM(fVel), fInputY, fTransmissionRatio, fEngineClutchLockRPM);
            fEngineTorque = cd_engine.GetMaxEngineTorque();
        }

        // get requested engine torque
        if (fInputY > 0.2f) fEngineTorque = fEngineTorque * fInputY;
        else fEngineTorque = 0.0f;

        // get requested wheel torques
        float[] faWheelTorques = cd_transmission.GetWheelTorques(fEngineTorque, WC);


        // get traction control torque updates
        if (cd_tractioncontrol != null) faWheelTorques = cd_tractioncontrol.GetTCReducedTorques(faWheelTorques, WC);

        // get requested brake torques
        float[] faBrakeTorques = cd_brakes.GetBrakeTorques(fInputY);

        // if handbrake is on, brake rear wheels
        if (fInputH > 0.1f) cd_brakes.ApplyHandbrake(faBrakeTorques);

        // Calculate a wheel rpm limit based on engine limit and transmission
        float fWheelRPMLimit = Mathf.Abs(cd_engine.GetEngineRPMMax / cd_transmission.GetTransmissionRatio()) * 1.01f;

        // check if wheel should be hitting engine rpm limit, if so, cut power to prevent over revving of wheel
        int iDrivenWheelID;
        for (int j = 0; j < cd_transmission.GetDrivenWheels.Count; j++)
        {
            iDrivenWheelID = cd_transmission.GetDrivenWheels[j];
            if (WC[cd_transmission.GetDrivenWheels[j]].rpm > fWheelRPMLimit) faWheelTorques[iDrivenWheelID] = 0.0f;
        }

        // update brake and motor torques on wheel colliders
        // RL RR FL FR
        // initialise all to 0.0f
        for (int i = 0; i < 4; i++)
        {
            WC[i].brakeTorque = faBrakeTorques[i];
            WC[i].motorTorque = faWheelTorques[i];
        }
        */
    }


    void Update()
    {
        Transform transWheel;

        /*
        // if transmission is manual then check for gear change request
        // Button up/down detection happens at display frequency, so the check is performed here
        if (cd_transmission != null)
        {
            if (!cd_transmission.GetAutomatic)
            {
                if (Input.GetButtonDown("GearShiftUp")) cd_transmission.GearShiftUp();
                if (Input.GetButtonDown("GearShiftDown")) cd_transmission.GearShiftDown();
            }
        }
        */

        // update wheel mesh positions to match wheel collider positions
        // slightly convoluted correction for tyre compression which must be corrected locally when WC position update is only available globally
        float fTyreOffset = 0.0f;

        for (int i = 0; i < 4; i++)
        {
            wC[i].GetWorldPose(out Vector3 wcPosition, out Quaternion wcRotation);
            transWheel = wC[i].gameObject.transform.GetChild(0);
            transWheel.transform.position = wcPosition;
            if (i < 2)
            {
                fTyreOffset = suspension.GetTyreRearOffset;
            }
            else
            {
                fTyreOffset = suspension.GetTyreFrontOffset;
            }
            transWheel.transform.localPosition = new Vector3(transWheel.transform.localPosition.x, transWheel.transform.localPosition.y - fTyreOffset, transWheel.transform.localPosition.z);
            transWheel.transform.rotation = wcRotation;
        }

   
    }

}
