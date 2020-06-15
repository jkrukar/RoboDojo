using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivetrainController : Singleton<DrivetrainController>
{
    private string logPrefix = "[Drivetrain] ";
    private Block activeBlock = null;
    Rigidbody botRigidBody;
    public float botDriveVeloctiy = 0.5f; //The current drive velocity expressed as a percentage of the max velocity 1.0
    public bool driveForward = false;
    public bool turning = false;
    public int directionPolarity = 1;
    private float maxDriveVelocity = 2.5f; //Max velocity at 100% is 500mm/sec or 0.5m/s
    private float maxTurnVelocity = Mathf.PI;  //Max angular velocity at 100% is 180'/s or pi rad/s

    // Start is called before the first frame update
    void Start()
    {
        botRigidBody = BlockParser.instance.botRigidBody;
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

    private void FixedUpdate()
    {

        if (driveForward)
        {          
            if (!botRigidBody.velocity.normalized.Equals(Vector3.zero))
            {
                botRigidBody.velocity = (maxDriveVelocity * botDriveVeloctiy) * botRigidBody.velocity.normalized;
            }
            else
            {
                botRigidBody.velocity = (maxDriveVelocity * botDriveVeloctiy) * Vector3.one;
            }
        }
    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);
        activeBlock = block;

        switch (block.type)
        {
            case "iq_drivetrain_drive":
                DriveForward(block);
                break;
            case "iq_drivetrain_drive_for":
                break;
            case "iq_drivetrain_turn":
                break;
            case "iq_motion_stop_driving":
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_velocity":
                SetDriveVelocity(block);
                break;
            case "iq_drivetrain_set_turn_velocity":
                break;
            case "iq_drivetrain_set_drive_stopping": //TODO: This is not fully implemented in VEX VR and I don't fully understand how to use it.
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_timeout": //TODO: not implemented
                block.finished = true;
                break;
        }

        
    }

    private void StopDriving()
    {
        driveForward = false;
        botRigidBody.velocity = Vector3.zero;
    }

    private void StopDriving(Block block)
    {
        StopDriving();
        block.finished = true;
    }

    private void DriveForward(Block block)
    {
        Debug.Log(logPrefix + "Drive forward");
        driveForward = true;
        block.finished = true;
    }

    private IEnumerator DriveForwardFor(Block block)
    {

        yield return null;
    }

    private void SetDriveVelocity(Block block)
    {
        float newVelocity = float.Parse(block.values[0].shadow.field.value);
        newVelocity /= 100;

        Debug.Log(logPrefix + "Set Drive Velocity to " + newVelocity);

        botDriveVeloctiy = newVelocity;
        block.finished = true;
    }






}
