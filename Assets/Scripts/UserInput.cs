using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{

    private float controllerInputX;
    private float controllerInputY;
    private float controllerInputReverse;
    private float controllerInputHandBrake;

    public float ControllerInputX
    {
        get { return controllerInputX; }
    }

    public float ControllerInputY
    {
        get { return controllerInputY; }
    }

    public float ControllerInputReverse
    {
        get { return controllerInputReverse; }
    }

    public float ControllerInputHandBrake
    {
        get { return controllerInputHandBrake; }
    }

    // Update is called once per frame
    void Update()
    {
        controllerInputX = Input.GetAxis("Horizontal");
        controllerInputY = Input.GetAxis("Vertical");
        controllerInputReverse = Input.GetAxis("Reverse");
        controllerInputHandBrake = Input.GetAxis("HandBrake");
    }

}
