using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tyres : MonoBehaviour
{
    public AudioClip acSlipLat;

    [SerializeField] private float slipLongFeedback = 0.3f;
    [SerializeField] private float slipLatFeedback = 0.08f;
    [SerializeField] private float tyreFrictionFront = 1.1f;
    [SerializeField] private float tyreFrictionRear = 1.1f;

    private WheelFrictionCurve[] awfcLong = new WheelFrictionCurve[4];
    private WheelFrictionCurve[] awfcLat = new WheelFrictionCurve[4];
    private AudioSource asSlipLat;
    private int[] aiParticleResetCount = new int[] { 0, 0, 0, 0 };
    private WheelCollider[] wC;
    private ParticleSystem[] particles;

    public WheelFrictionCurve[] GetWFCLong { get { return awfcLong; } }
    public WheelFrictionCurve[] GetWFCLat { get { return awfcLat; } }

    void Start()
    {

        // get wheelcolliders and particles
        wC = gameObject.GetComponentsInChildren<WheelCollider>();
        particles = gameObject.GetComponentsInChildren<ParticleSystem>();

        // WFC characteristics rear tyres
        for (int i = 0; i < 2; i++)
        {
            // longitudinal slip (% of longitudinal travel) versus normalised load
            awfcLong[i].extremumSlip = 0.15f;
            awfcLong[i].extremumValue = 1.0f;
            awfcLong[i].asymptoteSlip = 0.70f;
            awfcLong[i].asymptoteValue = 0.60f;
            awfcLong[i].stiffness = tyreFrictionRear;
            // lateral slip = radians slip versus normalised load
            awfcLat[i].extremumSlip = 0.11f;
            awfcLat[i].extremumValue = 1.0f;
            awfcLat[i].asymptoteSlip = 0.80f;
            awfcLat[i].asymptoteValue = 0.70f;
            awfcLat[i].stiffness = tyreFrictionRear;
        }

        // WFC characteristics front tyres
        for (int i = 2; i < 4; i++)
        {
            // longitudinal slip (% of longitudinal travel) versus normalised load
            awfcLong[i].extremumSlip = 0.15f;
            awfcLong[i].extremumValue = 1.0f;
            awfcLong[i].asymptoteSlip = 0.70f;
            awfcLong[i].asymptoteValue = 0.70f;
            awfcLong[i].stiffness = tyreFrictionFront;
            // lateral slip = radians slip versus normalised load
            awfcLat[i].extremumSlip = 0.11f;
            awfcLat[i].extremumValue = 1.0f;
            awfcLat[i].asymptoteSlip = 0.80f;
            awfcLat[i].asymptoteValue = 0.70f;
            awfcLat[i].stiffness = tyreFrictionFront;
        }

        // Assign the WFC data to the wheel colliders
        // WC sequence RL RR FL FR

        for (int i = 0; i < 4; i++)
        {
            wC[i].ConfigureVehicleSubsteps(30, 8, 20);
            if (i < 2)
            {
                wC[i].forwardFriction = awfcLong[i];
                wC[i].sidewaysFriction = awfcLat[i];
            }
            else
            {
                wC[i].forwardFriction = awfcLong[i];
                wC[i].sidewaysFriction = awfcLat[i];
            }
        }

        /*
        asSlipLat = gameObject.AddComponent<AudioSource>();
        asSlipLat.clip = acSlipLat;
        asSlipLat.volume = 0;
        asSlipLat.loop = true;
        asSlipLat.Play();
        */
    }

    /*
    void Update()
    {
        // provide feedback for longitudinal and lateral slip
        float fSlipLong, fSlipLat;
        float fSlipLatMax = 0.0f;
        for (int i = 0; i < 4; i++)
        {
            wC[i].GetGroundHit(out WheelHit whContactPatch);
            fSlipLong = Mathf.Abs(whContactPatch.forwardSlip);
            fSlipLat = Mathf.Abs(whContactPatch.sidewaysSlip);
            if (fSlipLong > fSlipLongFeedback && cd_master.GetVel > 1.0f && aiParticleResetCount[i] == 0)
            {
                particles[i].gameObject.transform.rotation = Quaternion.LookRotation(cd_master.GetRB.velocity) * Quaternion.Euler(Vector3.up * 180.0f);
                particles[i].Play();
                aiParticleResetCount[i] = 10;
            }
            else
            {
                if (aiParticleResetCount[i] > 0) aiParticleResetCount[i]--;
                if (particles[i].isPlaying && aiParticleResetCount[i] <= 0)
                {
                    particles[i].Stop();
                    aiParticleResetCount[i] = 0;
                }
            }
            if (fSlipLat > fSlipLatFeedback)
            {
                asSlipLat.volume = 0.2f;
                if (fSlipLat > fSlipLatMax) fSlipLatMax = fSlipLat;
            }

        }

        if (fSlipLatMax > fSlipLatFeedback) fSlipLatMax = fSlipLatMax / 1.57f;
        asSlipLat.volume = fSlipLatMax;
    }*/


}
