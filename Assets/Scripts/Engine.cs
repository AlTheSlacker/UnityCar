using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCar
{

    public class Engine : MonoBehaviour
    {

        [SerializeField] private float engineRPMIdle = 800.0f;
        [SerializeField] private float enginePowerIdle = 10.0f;
        [SerializeField] private float engineRPMLowPowerBand = 2500.0f;
        [SerializeField] private float enginePowerLowPowerBand = 50.0f;
        [SerializeField] private float engineRPMMaxPower = 5500.0f;
        [SerializeField] private float enginePowerMaxPower = 120.0f;
        [SerializeField] private float engineRPMMax = 6000.0f;
        [SerializeField] private float enginePowerMaxRPM = 100.0f;

        private readonly float[,] engineTorque = new float[4, 2];
        private float rPM;

        public float GetRPM { get { return rPM; } }
        public float GetEngineRPMMaxPower { get { return engineRPMMaxPower; } }
        public float GetEngineRPMMax { get { return engineRPMMax; } }

        void Start()
        {
            // generate engine torque curves
            EngineTorqueSetup();
        }

        public float GetMaxEngineTorque()
        {
            float fTorque;
            if (rPM <= engineTorque[0, 0]) fTorque = 0.0f;
            else if (rPM > engineTorque[0, 0] && rPM <= engineTorque[1, 0])
            {
                fTorque = Mathf.Lerp(engineTorque[1, 1], engineTorque[0, 1], (rPM - engineTorque[0, 0]) / (engineTorque[1, 0] - engineTorque[0, 0]));
            }
            else if (rPM > engineTorque[1, 0] && rPM <= engineTorque[2, 0])
            {
                fTorque = Mathf.Lerp(engineTorque[2, 1], engineTorque[1, 1], (rPM - engineTorque[1, 0]) / (engineTorque[2, 0] - engineTorque[1, 0]));
            }
            else if (rPM > engineTorque[2, 0] && rPM <= engineTorque[3, 0])
            {
                fTorque = Mathf.Lerp(engineTorque[3, 1], engineTorque[2, 1], (rPM - engineTorque[2, 0]) / (engineTorque[3, 0] - engineTorque[2, 0]));
            }
            else
            {
                fTorque = engineTorque[3, 1];
            }
            return fTorque;
        }


        float PowerToTorque(float power, float rPM)
        {
            // There is a fixed relationship between power and torque
            // This converts hp -> kW as well as doing a torque conversion
            return power * 7120.54f / rPM;
        }

        void EngineTorqueSetup()
        {
            // add RPM values for the torque curve array
            engineTorque[0, 0] = engineRPMIdle;
            engineTorque[1, 0] = engineRPMLowPowerBand;
            engineTorque[2, 0] = engineRPMMaxPower;
            engineTorque[3, 0] = engineRPMMax;
            // add torque values for the torque curve array
            engineTorque[0, 1] = PowerToTorque(enginePowerIdle, engineRPMIdle);
            engineTorque[1, 1] = PowerToTorque(enginePowerLowPowerBand, engineRPMLowPowerBand);
            engineTorque[2, 1] = PowerToTorque(enginePowerMaxPower, engineRPMMaxPower);
            engineTorque[3, 1] = PowerToTorque(enginePowerMaxRPM, engineRPMMax);
        }

        public void UpdateEngineSpeedRPM(float wheelRPM, float inputY, float gearRatio, float engineClutchLockRPM)
        {
            // calculate engine speed as a function of vehicle speed and transmission gearing
            rPM = wheelRPM * gearRatio;
            // do some sanity checks and simulate clutch slip if required
            if (rPM < engineClutchLockRPM && inputY > 0.01f) rPM = engineClutchLockRPM;
            else if (rPM < engineRPMIdle) rPM = engineRPMIdle;
            else if (rPM > engineRPMMax) rPM = engineRPMMax;
        }

    }
}