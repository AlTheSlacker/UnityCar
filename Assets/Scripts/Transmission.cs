using System.Collections.Generic;
using UnityEngine;

public class Transmission : MonoBehaviour
{

    [SerializeField] private float engineClutchLockRPM = 3000.0f;
    [SerializeField] private bool automatic = true;
    [SerializeField] private int numberOfGears = 5;
    [SerializeField] private float gearRatio1 = 4.23f;
    [SerializeField] private float gearRatio2 = 0.0f;
    [SerializeField] private float gearRatio3 = 0.0f;
    [SerializeField] private float gearRatio4 = 0.0f;
    [SerializeField] private float gearRatio5 = 0.0f;
    [SerializeField] private float gearRatio6 = 0.0f;
    [SerializeField] private float gearRatio7 = 0.0f;
    [SerializeField] private float gearRatio8 = 0.0f;
    [SerializeField] private float gearRatio9 = 0.0f;
    [SerializeField] private float gearRatio10 = 0.0f;
    [SerializeField] private float finalDriveRatio = 3.0f;
    [SerializeField] private float diffSlipLimitFront = 1.0f;
    [SerializeField] private float diffTransferLimitFront = 1.0f;
    [SerializeField] private float diffSlipLimitRear = 1.0f;
    [SerializeField] private float diffTransferLimitRear = 1.0f;
    [SerializeField]
    private enum DrivenWheels
    {
        FWD = 1,
        RWD = 2,
        FourWD = 4
    }
    [SerializeField] private DrivenWheels drive = DrivenWheels.RWD;
    [SerializeField] private float fourWDFrtTorque = 0.4f;

    private int gearCurrent = 1;
    private float[] gearTotalRatios = new float[11];
    private List<int> drivenWheels = new List<int>();

    public int GetGearCurrent { get { return gearCurrent; } }
    public bool GetAutomatic { get { return automatic; } }
    public float GetEngineClutchLockRPM { get { return engineClutchLockRPM; } }
    public float GetDiffSlipLimitRear { get { return diffSlipLimitRear; } }
    public float GetDiffSlipLimitFront { get { return diffSlipLimitFront; } }
    public float GetDiffTransferLimitRear { get { return diffTransferLimitRear; } }
    public float GetDiffTransferLimitFront { get { return diffTransferLimitFront; } }
    public List<int> GetDrivenWheels { get { return drivenWheels; } }


    void Start()
    {

        // Limited to a maximum of 10 forward gears
        if (numberOfGears > 10) numberOfGears = 10;

        // set up transmission
        float[] gearRatios = new float[11];
        // Manually set up a reverse gear
        gearRatios[0] = -3.4f;
        // Check we have a first gear ratio defined, if not fall back to something normal
        if (gearRatio1 == 0.0) gearRatio1 = 4.2f;
        gearRatios[1] = gearRatio1;
        gearRatios[2] = gearRatio2;
        gearRatios[3] = gearRatio3;
        gearRatios[4] = gearRatio4;
        gearRatios[5] = gearRatio5;
        gearRatios[6] = gearRatio6;
        gearRatios[7] = gearRatio7;
        gearRatios[8] = gearRatio8;
        gearRatios[9] = gearRatio9;
        gearRatios[10] = gearRatio10;

        // If gears are not defined then generate a progressively spaced gearbox from the defined 1st gear to a 1:1 final gear.
        // set up an array for the combined gearbox and final drive
        // note: this will fill in missing gear ratios and leave predefined ones... this has no sanity checking.
        gearTotalRatios[0] = gearRatios[0] * finalDriveRatio;
        float geometricFactor = Mathf.Pow(1.0f / gearRatio1, 1.0f / (numberOfGears - 1));
        float progressionFactor = 1.15f;
        for (int i = 1; i <= numberOfGears; i++)
        {
            if (gearRatios[i] <= 0.01f)
            {
                gearRatios[i] = gearRatio1 * Mathf.Pow(geometricFactor, i - 1);
                if (i != 1 && i != numberOfGears) gearRatios[i] = gearRatios[i] / progressionFactor;
            }
            gearTotalRatios[i] = gearRatios[i] * finalDriveRatio;
        }

        // set up driven wheel list
        if ((int)drive == 2 || (int)drive == 4) //RWD or 4WD
        {
            drivenWheels.Add(0);
            drivenWheels.Add(1);
        }
        if ((int)drive == 1 || (int)drive == 4) //RWD or 4WD
        {
            drivenWheels.Add(2);
            drivenWheels.Add(3);
        }

    }

    public void SetGear(float fWheelRPM)
    {
        Engine engine = GetComponent<Engine>();
        if (fWheelRPM == 0.0f) fWheelRPM = 0.1f;
        float fMaxTotalRatio = Mathf.Abs(engine.GetEngineRPMMaxPower / fWheelRPM);
        int iGear = gearCurrent;
        for (int i = numberOfGears; i > 0; i--)
        {
            if (fMaxTotalRatio > gearTotalRatios[i]) iGear = i;
        }
        gearCurrent = iGear;
    }


    public void GearShiftUp()
    {
        if (gearCurrent < numberOfGears) gearCurrent++;
    }


    public void GearShiftDown()
    {
        if (gearCurrent > 0) gearCurrent--;
    }

    public void SelectReverse()
    {
        gearCurrent = 0;
    }

    public float GetTransmissionRatio()
    {
        return gearTotalRatios[gearCurrent];
    }

    public float[] GetWheelTorques(float engineTorque, WheelCollider[] wC)
    {
        float[] wheelTorques = new float[4];
        float[] diffTorques = new float[2];
        float gearboxTorque = engineTorque * GetTransmissionRatio();
        float torqueSplitFront = 1.0f;
        float torqueSplitRear = 1.0f;
        if ((int)drive == 4)
        {
            torqueSplitFront = fourWDFrtTorque;
            torqueSplitRear = 1.0f - fourWDFrtTorque;
        }
        if ((int)drive > 1)
        {
            wC[0].GetGroundHit(out WheelHit contactPatchLHR);
            wC[1].GetGroundHit(out WheelHit contactPatchRHR);
            diffTorques = DiffOutput(Mathf.Abs(contactPatchLHR.forwardSlip), Mathf.Abs(contactPatchRHR.forwardSlip), diffSlipLimitRear, gearboxTorque * torqueSplitRear, diffTransferLimitRear);
            wheelTorques[0] = diffTorques[0];
            wheelTorques[1] = diffTorques[1];
        }
        if ((int)drive == 1 || (int)drive == 4)
        {
            wC[2].GetGroundHit(out WheelHit contactPatchLHF);
            wC[3].GetGroundHit(out WheelHit contactPatchRHF);
            diffTorques = DiffOutput(Mathf.Abs(contactPatchLHF.forwardSlip), Mathf.Abs(contactPatchRHF.forwardSlip), diffSlipLimitFront, gearboxTorque * torqueSplitFront, diffTransferLimitFront);
            wheelTorques[2] = diffTorques[0];
            wheelTorques[3] = diffTorques[1];
        }
        return wheelTorques;
    }


    private float[] DiffOutput(float slipLH, float slipRH, float maxSlip, float torque, float maxTrans)
    {
        float[] output = new float[2];
        float slipDifferential = slipRH - slipLH;
        float singleWheelBaseTorque = 0.5f * torque;
        float torqueTransfer = 0.0f;

        if (maxSlip != 0.0f)
        {
            if (Mathf.Abs(slipDifferential) <= Mathf.Abs(maxSlip))
            {
                torqueTransfer = slipDifferential / maxSlip;
            }
            else
            {
                torqueTransfer = 1.0f * Mathf.Sign(slipDifferential) * Mathf.Sign(maxSlip);
            }
        }
        float torqueAdjustment = singleWheelBaseTorque * torqueTransfer;
        output[0] = singleWheelBaseTorque - torqueAdjustment;
        output[1] = singleWheelBaseTorque + torqueAdjustment;
        return output;
    }


}
