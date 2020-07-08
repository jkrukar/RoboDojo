using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivetrainController : Singleton<DrivetrainController>
{
    private string logPrefix = "[Drivetrain] ";
    private Block activeBlock = null;
    Rigidbody botRigidBody;
    public float botDriveVelocity = 0.5f; //The current drive velocity expressed as a percentage of the max velocity 1.0
    public float botTurnVelocity = 0.5f; //The current turn velocity expressed as a percentage of the max velocity 1.0
    public bool driving = false;
    public bool turning = false;
    public int drivePolarity = 1;
    public int turnPolarity = 1;
    private float physicsMaxDriveVelocity = 3.0f; //Max velocity at 100% is 500mm/sec or 0.5m/s
    private float maxDriveVelocity = 2.0f; //Max velocity at 100% is 500mm/sec or 0.5m/s
    //private float maxTurnVelocity = 180.0f;  //Max angular velocity at 100% is 180'/s or pi rad/s
    private float physicsMaxTurnVelocity = 7.15f;  //Max angular velocity at 100% is 180'/s or pi rad/s
    private float maxTurnVelocity = 180f;  //Max angular velocity at 100% is 180'/s or pi rad/s
    public bool usePhysics = false;
    public bool driveTimedOut = false;

    // Start is called before the first frame update
    void Start()
    {
        botRigidBody = BlockParser.instance.botRigidBody;
    }

    // Update is called once per frame
    void Update()
    {
        if (BlockParser.gamePlaying)
        {
            if (!usePhysics)
            {
                if (driving)
                {
                    botRigidBody.gameObject.transform.Translate(Vector3.forward * (botDriveVelocity * maxDriveVelocity) * drivePolarity * Time.deltaTime);
                }

                if (turning)
                {
                    botRigidBody.gameObject.transform.Rotate(Vector3.up * (turnPolarity * (botTurnVelocity * maxTurnVelocity) * Time.deltaTime));
                }
            }
        }

        if (activeBlock != null && !activeBlock.statementBlock)
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
        if (BlockParser.gamePlaying)
        {
            if (usePhysics)
            {
                if (driving)
                {
                    botRigidBody.velocity = (physicsMaxDriveVelocity * botDriveVelocity) * botRigidBody.transform.forward * drivePolarity;
                }

                if (turning)
                {
                    //Quaternion deltaRotation = Quaternion.Euler((maxTurnVelocity * Vector3.up) * Time.deltaTime);
                    //botRigidBody.MoveRotation(botRigidBody.rotation * deltaRotation);

                    botRigidBody.angularVelocity = (physicsMaxTurnVelocity * botTurnVelocity) * botRigidBody.transform.up * turnPolarity; //USE THIS ONE
                    //Debug.Log("angularVelocity = " + botRigidBody.angularVelocity);

                    //botRigidBody.transform.rotation = botRigidBody.transform.rotation * Quaternion.AngleAxis((maxTurnVelocity * botTurnVeloctiy) * Time.deltaTime, Vector3.up);
                }
            }
        }
        else
        {
            if (usePhysics)
            {
                botRigidBody.velocity = Vector3.zero;
                botRigidBody.angularVelocity = Vector3.zero;
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
            case "iq_drivetrain_turn_to_heading":
                TurnToHeading(block);
                break;
            case "iq_drivetrain_set_drive_heading":
                TurnToHeading(block);
                break;
            case "iq_drivetrain_turn_to_rotation": //TODO: not implemented
                TurnToRotation(block);
                break;
            case "iq_drivetrain_set_drive_rotation": //TODO: not implemented
                TurnToRotation(block);
                break;
            case "iq_drivetrain_set_drive_stopping": //TODO: This is not fully implemented in VEX VR and I don't fully understand how to use it.
                StopDriving(block);
                break;
            case "iq_drivetrain_set_drive_timeout":
                StartCoroutine(SetDriveTimeout(block));
                break;
        }        
    }

    private void TurnToHeading(Block block)
    {
        float degrees = BlockParser.instance.ResolveBlockValue(block.values[0]);
        
        int turnPolarity = 1;

        if(block.fields.Count == 0)
        {
            BlockField fakeField = new BlockField();
            fakeField.value = "True";
            block.fields.Add(fakeField);
        }

        bool andDontWait = bool.Parse(block.fields[0].value);

        //Transform this block into a TurnFor block and use existing logic
        float deltaDegreesCW = 0;
        float deltaDegreesCCW = 0;
        float bestDeltaDegrees = 0;

        if (degrees > botRigidBody.transform.rotation.eulerAngles.y)
        {
            deltaDegreesCW = degrees - botRigidBody.transform.rotation.eulerAngles.y;
            deltaDegreesCCW = (360 - degrees) + botRigidBody.transform.rotation.eulerAngles.y;
        }
        else
        {
            deltaDegreesCW = degrees + (360 - botRigidBody.transform.rotation.eulerAngles.y);
            deltaDegreesCCW = botRigidBody.transform.rotation.eulerAngles.y - degrees;
        }

        if (deltaDegreesCCW < deltaDegreesCW)
        {
            bestDeltaDegrees = deltaDegreesCCW;
            turnPolarity = -1;
        }
        else
        {
            bestDeltaDegrees = deltaDegreesCW;
        }

        BlockField turnDirectionField = new BlockField();
        turnDirectionField.name = "TURNDIRECTION";

        if (turnPolarity > 0)
        {
            turnDirectionField.value = "right";
        }
        else
        {
            turnDirectionField.value = "left";
        }

        block.fields.Add(turnDirectionField);
        block.values[0].block = null;

        BlockValue newVal = new BlockValue();
        BlockShadow newShadow = new BlockShadow();
        BlockField newField = new BlockField();

        newField.value = bestDeltaDegrees.ToString();
        newShadow.field = newField;
        newVal.shadow = newShadow;
        block.values[0] = newVal;

        StartCoroutine(TurnFor(block));
    }

    private void TurnToRotation(Block block)
    {
        float degrees = BlockParser.instance.ResolveBlockValue(block.values[0]);

        //Transform this block into a TurnFor block and use existing logic

        if (degrees < 0)
        {
            degrees = 180 + (180 + degrees);
        }

        block.values[0].block = null;

        BlockValue newVal = new BlockValue();
        BlockShadow newShadow = new BlockShadow();
        BlockField newField = new BlockField();

        newField.value = degrees.ToString();
        newShadow.field = newField;
        newVal.shadow = newShadow;
        block.values[0] = newVal;

        TurnToHeading(block);
    }

    private IEnumerator SetDriveTimeout(Block block)
    {
        float timeout = BlockParser.instance.ResolveBlockValue(block.values[0]);
        block.finished = true;

        yield return new WaitForSeconds(timeout);

        driveTimedOut = true;
        StopDriving();

        yield return null;
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

        Debug.Log(logPrefix + " Drive (" + polarity + "): for " + amount + " degrees and dontwait=" + andDontWait);

        float targetRotation = botRigidBody.transform.rotation.eulerAngles.y + (amount * turnPolarity);

        turning = true;
        bool doneTurning = false;

        if (andDontWait)
        {
            Debug.Log("Done Turning!");
            block.finished = true;
        }

        while (!doneTurning)
        {
            if (!turning) //Handle concurrent block execution for - and dont wait TODO: Could be problematic if a distance sensor tries to stop it or something!!!
            {
                turning = true;
            }

            float degreesTurned = Vector3.SignedAngle(originalRotation, botRigidBody.transform.forward, Vector3.up);

            if(degreesTurned < 0 && turnPolarity > 0)
            {
                degreesTurned = 360 + degreesTurned;
            }
            else if(turnPolarity < 0)
            {
                if(degreesTurned > 0)
                {
                    degreesTurned = 180 + (180 - degreesTurned);
                }
                else
                {
                    degreesTurned = degreesTurned * -1;
                }
            }

            //Debug.Log("degreesTurned= " + originalRotation);
            //Debug.Log("degreesTurned= " + botRigidBody.transform.forward);
            //Debug.Log("degreesTurned= " + degreesTurned);
            //Debug.Log("amount= " + amount);

            if (degreesTurned >= amount)
            {
                doneTurning = true;
                StopDriving();

                Vector3 botEulerRotation = botRigidBody.transform.rotation.eulerAngles;
                botRigidBody.transform.rotation = Quaternion.Euler(new Vector3(botEulerRotation.x,targetRotation, botEulerRotation.z));

                if (!andDontWait)
                {
                    Debug.Log("Done Turning!");
                    block.finished = true;
                }
            }

            if (driveTimedOut)
            {
                driveTimedOut = false;
                doneTurning = true;
                StopDriving();
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

        botTurnVelocity = newVelocity;
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
            Debug.Log("Done Driving!");
            block.finished = true;
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
                    Debug.Log("Done Driving!");
                    block.finished = true;
                }         
            }

            if (driveTimedOut)
            {
                driveTimedOut = false;
                doneDriving = true;
                StopDriving();
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

        botDriveVelocity = newVelocity;
        block.finished = true;
    }
}
