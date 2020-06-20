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
                StartCoroutine(DriveFor(block));
                break;
            case "iq_drivetrain_turn":
                Turn(block);
                break;
            case "iq_drivetrain_turn_for":
                StartCoroutine(TurnFor(block));
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
            case "iq_drivetrain_turn_to_heading": //TODO: not implemented
                block.finished = true;
                break;
            case "iq_drivetrain_turn_to_rotation": //TODO: not implemented
                block.finished = true;
                break;
            case "iq_drivetrain_set_drive_stopping": //TODO: This is not fully implemented in VEX VR and I don't fully understand how to use it.
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_timeout": //TODO: not implemented
                block.finished = true;
                break;
        }

        
    }

    private IEnumerator TurnFor(Block block)
    {
        Vector3 originalRotation = botRigidBody.transform.forward;
        string polarity = "";
        bool andDontWait = false;
        float amount = 0f;

        foreach (BlockField field in block.fields)
        {
            switch (field.name)
            {
                case "TURNDIRECTION":
                    polarity = field.value;
                    break;
                case "anddontwait_mutator":
                    andDontWait = bool.Parse(field.value);
                    break;
            }
        }

        amount = BlockParser.instance.ResolveBlockValue(block.values[0]);

        if (polarity == "right")
        {
            turnPolarity = 1;
        }
        else
        {
            turnPolarity = -1;
        }

        Debug.Log(logPrefix + " Drive (" + polarity + "): for " + amount + "degrees and dontwait=" + andDontWait);

        turning = true;
        bool doneTurning = false;

        if (andDontWait)
        {
            activeBlock.finished = true;
        }

        while (!doneTurning)
        {
            if (!turning) //Handle concurrent block execution for - and dont wait TODO: Could be problematic if a distance sensor tries to stop it or something!!!
            {
                turning = true;
            }

            float degreesTurned = Vector3.Angle(originalRotation, botRigidBody.transform.forward);

            Debug.Log("degreesTurned= " + originalRotation);
            Debug.Log("degreesTurned= " + botRigidBody.transform.forward);
            Debug.Log("degreesTurned= " + degreesTurned);

            if (degreesTurned >= amount)
            {
                doneTurning = true;
                StopDriving();

                if (!andDontWait)
                {
                    activeBlock.finished = true;
                }
            }

            yield return null;
        }

        yield return null;
    }

    private void SetTurnVelocity(Block block)
    {
        float newVelocity = BlockParser.instance.ResolveBlockValue(block.values[0]);
        newVelocity /= 100;

        Debug.Log(logPrefix + "Set Turn Velocity to " + newVelocity);

        botTurnVeloctiy = newVelocity;
        block.finished = true;
    }

    private void Turn(Block block)
    {
        Debug.Log(logPrefix + "Turn");

        string polarity = block.fields[0].value;

        if (polarity == "right")
        {
            drivePolarity = 1;
        }
        else
        {
            drivePolarity = -1;
        }

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

        string polarity = block.fields[0].value;

        if(polarity == "fwd")
        {
            drivePolarity = 1;
        }
        else
        {
            drivePolarity = -1;
        }

        driving = true;
        block.finished = true;
    }

    private IEnumerator DriveFor(Block block)
    {
        Vector3 originalPosition = botRigidBody.transform.position;
        string polarity = "";
        string units = "";
        bool andDontWait = false;
        float amount = 0f;

        foreach(BlockField field in block.fields)
        {
            switch (field.name)
            {
                case "DIRECTION":
                    polarity = field.value;
                    break;
                case "UNITS":
                    units = field.value;
                    break;
                case "anddontwait_mutator":
                    andDontWait = bool.Parse(field.value);
                    break;
            }
        }

        amount = BlockParser.instance.ResolveBlockValue(block.values[0]);
        amount *= 0.005f; //Convert to Unity units

        if (units == "in")
        {
            amount *= 25.4f; //Convert inches to mm
        }

        if (polarity == "fwd")
        {
            drivePolarity = 1;
        }
        else
        {
            drivePolarity = -1;
        }

        Debug.Log(logPrefix + " Drive (" + polarity + "): for " + amount + " " + units + " and dontwait=" + andDontWait);

        driving = true;
        bool doneDriving = false;

        if (andDontWait)
        {
            activeBlock.finished = true;
        }

        while (!doneDriving)
        {
            if (!driving) //Handle concurrent block execution for - and dont wait
            {
                driving = true;
            }

            float distanceDriven = Vector3.Distance(originalPosition, botRigidBody.transform.position);
            //Debug.Log("distanceDriven= " + distanceDriven);

            if (distanceDriven >= amount)
            {
                doneDriving = true;
                StopDriving();

                if (!andDontWait)
                {
                    activeBlock.finished = true;
                }           
            }

            yield return null;
        }

        yield return null;
    }

    private void SetDriveVelocity(Block block)
    {
        float newVelocity = BlockParser.instance.ResolveBlockValue(block.values[0]);
        newVelocity /= 100;

        Debug.Log(logPrefix + "Set Drive Velocity to " + newVelocity);

        botDriveVeloctiy = newVelocity;
        block.finished = true;
    }
}
