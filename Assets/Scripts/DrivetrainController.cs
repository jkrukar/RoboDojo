using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivetrainController : Singleton<DrivetrainController>
{
    private string logPrefix = "[Drivetrain] ";
    private Block activeBlock = null;
    Rigidbody botRigidBody;
    public float botDriveVeloctiy = 0.5f; //The current drive velocity expressed as a percentage of the max velocity 1.0
    public float botTurnVeloctiy = 0.5f; //The current turn velocity expressed as a percentage of the max velocity 1.0
    public bool driving = false;
    public bool turning = false;
    public int drivePolarity = 1;
    public int turnPolarity = 1;
    private float maxDriveVelocity = 3.0f; //Max velocity at 100% is 500mm/sec or 0.5m/s
    //private float maxTurnVelocity = 180.0f;  //Max angular velocity at 100% is 180'/s or pi rad/s
    private float maxTurnVelocity = 7.15f;  //Max angular velocity at 100% is 180'/s or pi rad/s

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

        if (driving)
        {
            botRigidBody.velocity = (maxDriveVelocity * botDriveVeloctiy) * botRigidBody.transform.forward * drivePolarity;
        }

        if (turning)
        {
            //Quaternion deltaRotation = Quaternion.Euler((maxTurnVelocity * Vector3.up) * Time.deltaTime);
            //botRigidBody.MoveRotation(botRigidBody.rotation * deltaRotation);

            botRigidBody.angularVelocity = (maxTurnVelocity * botTurnVeloctiy) * botRigidBody.transform.up * turnPolarity;
            Debug.Log("angularVelocity = " + botRigidBody.angularVelocity);



            //botRigidBody.transform.rotation = botRigidBody.transform.rotation * Quaternion.AngleAxis((maxTurnVelocity * botTurnVeloctiy) * Time.deltaTime, Vector3.up);
        }

    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);
        activeBlock = block;

        switch (block.type)
        {
            case "iq_drivetrain_drive":
                Drive(block);
                break;
            case "iq_drivetrain_drive_for":
                break;
            case "iq_drivetrain_turn":
                Turn(block);
                break;
            case "iq_motion_stop_driving":
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_velocity":
                SetDriveVelocity(block);
                break;
            case "iq_drivetrain_set_turn_velocity":
                SetTurnVelocity(block);
                break;
            case "iq_drivetrain_set_drive_stopping": //TODO: This is not fully implemented in VEX VR and I don't fully understand how to use it.
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_timeout": //TODO: not implemented
                block.finished = true;
                break;
        }

        
    }

    private void SetTurnVelocity(Block block)
    {
        float newVelocity = float.Parse(block.values[0].shadow.field.value);
        newVelocity /= 100;

        Debug.Log(logPrefix + "Set Turn Velocity to " + newVelocity);

        botTurnVeloctiy = newVelocity;
        block.finished = true;
    }

    private void Turn(Block block)
    {
        Debug.Log(logPrefix + "Turn");
        turning = true;
        block.finished = true;
    }

    private void StopDriving()
    {
        driving = false;
        turning = false;
        botRigidBody.velocity = Vector3.zero;
        botRigidBody.angularVelocity = Vector3.zero;
    }

    private void StopDriving(Block block)
    {
        StopDriving();
        block.finished = true;
    }

    private void Drive(Block block)
    {
        Debug.Log(logPrefix + "Drive");
        driving = true;
        block.finished = true;
    }

    private IEnumerator DriveFor(Block block)
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
