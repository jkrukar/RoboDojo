using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivetrainController : Singleton<DrivetrainController>
{
    private string logPrefix = "[Drivetrain] ";
    private Block activeBlock = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBlock != null)
        {
            if (activeBlock.finished)
            {
                if(activeBlock.nextBlock != null)
                {
                    //Debug.Log("Push block: " + activeBlock.nextBlock.type);
                    BlockParser.instance.blockStack.Push(activeBlock.nextBlock);
                }                
                
                activeBlock = null;
            }
        }

        BlockParser.instance.ReceiveControllerReadySignal();
    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);
        activeBlock = block;

        switch (block.type)
        {
            case "iq_drivetrain_drive":
                break;
            case "iq_drivetrain_drive_for":
                break;
            case "iq_drivetrain_turn":
                break;
            case "iq_motion_stop_driving":
                break;
            case "iq_drivetrain_set_drive_velocity":
                break;
            case "iq_drivetrain_set_turn_velocity":
                break;
            case "iq_drivetrain_set_drive_stopping":
                break;
            case "iq_drivetrain_set_drive_timeout":
                break;
        }

        block.finished = true;
    }
}
