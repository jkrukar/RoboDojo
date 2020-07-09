using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (!BlockParser.autonomous)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("Forward!");
                DrivetrainController.instance.drivePolarity = 1;
                DrivetrainController.instance.driving = true;
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                Debug.Log("Stop!");
                DrivetrainController.instance.driving = false;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                DrivetrainController.instance.drivePolarity = -1;
                DrivetrainController.instance.driving = true;
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                DrivetrainController.instance.driving = false;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                DrivetrainController.instance.turnPolarity = -1;
                DrivetrainController.instance.turning = true;
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                DrivetrainController.instance.turning = false;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                DrivetrainController.instance.turnPolarity = 1;
                DrivetrainController.instance.turning = true;
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                DrivetrainController.instance.turning = false;
            }
        }        
    }
}
