using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensingController : Singleton<SensingController>
{
    private string logPrefix = "[Sensing] ";

    private GameObject distanceSensor;
    public float distanceSensorValue0;
    public float distanceSensorValue45;
    public bool physicallyConstrained = false; //If true, the distance sensor reading will be clamped between [50mm, 1m]
    private string distanceSensorName0;
    private string distanceSensorName45;

    private GameObject leftBumper;
    private GameObject rightBumper;

    private string leftBumperName;
    private string rightBumperName;
    public BumperSensor leftBumperSensor;
    public BumperSensor rightBumperSensor;

    // Start is called before the first frame update
    void Start()
    {
        distanceSensor = GameObject.Find("DistanceSensor");
        leftBumper = GameObject.Find("LeftBumper");
        rightBumper = GameObject.Find("RightBumper");

        leftBumperSensor = leftBumper.GetComponent<BumperSensor>();
        rightBumperSensor = rightBumper.GetComponent<BumperSensor>();

        List<string> distanceSensors = BlockParser.instance.GetSensorsOfType("Distance");
        List<string> bumperSensors = BlockParser.instance.GetSensorsOfType("Bumper");

        distanceSensors.Sort();

        if(distanceSensors.Count > 0)
        {
            distanceSensorName0 = distanceSensors[0];

            if(distanceSensors.Count > 1)
            {
                distanceSensorName45 = distanceSensors[1];
            }
        }

        bumperSensors.Sort();

        if (bumperSensors.Count > 0)
        {
            leftBumperName = bumperSensors[0];

            if (bumperSensors.Count > 1)
            {
                rightBumperName = bumperSensors[1];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetDistanceSensorReading();

        BlockParser.instance.ReceiveControllerReadySignal();

        Debug.Log("rightBumperState = " + rightBumperSensor.collisionState);
        Debug.Log("leftBumperState = " + leftBumperSensor.collisionState);

    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);

        switch (block.type)
        {
            case "iq_sensing_distance_from":
                break;
            case "iq_sensing_object_in_front":
                break;
            case "iq_events_when_timer":
                break;
            case "iq_events_when_started":
                break;
            case "iq_events_when_broadcasted":
                break;
        }
    }

    public float ResolveSensorFloatValue(Block block)
    {
        float result = 0;

        if (block != null)
        {
            switch (block.type)
            {
                case "iq_sensing_distance_from":

                    Debug.Log("ResolveSensorFloatValue");

                    if (block.fields[0].value == distanceSensorName0)
                    {
                        Debug.Log("\tdistanceSensorValue0=" + distanceSensorValue0);
                        result = distanceSensorValue0;
                    }
                    else
                    {
                        Debug.Log("\tdistanceSensorValue0=" + distanceSensorValue45);
                        result = distanceSensorValue45;
                    }

                    result *= 1000;

                    if (block.fields[1].value != "mm")
                    {

                        result *= 0.0393701f; //convert from mm to inches
                        Debug.Log("\tconvert to inches!=" + result);
                    }
                    break;
            }
        }

        return result;
    }

    public bool ResolveSensorBoolValue(Block block)
    {
        bool result = false;

        if(block != null)
        {
            switch (block.type)
            {
                case "iq_sensing_object_in_front":

                    float distance = 0;

                    if (block.fields[0].value == distanceSensorName0)
                    {
                        distance = distanceSensorValue0;
                    }
                    else
                    {
                        distance = distanceSensorValue45;
                    }

                    if (distance < 1)
                    {
                        result = true;
                    }

                    Debug.Log("ResolveSensorBoolValue: Distance= " + distance + " -> " + result);

                    break;
                case "iq_sensing_pressing_bumper":

                    if (block.fields[0].value == leftBumperName)
                    {
                        //Debug.Log("leftBumperState = " + leftBumperSensor.collisionState);
                        result = leftBumperSensor.collisionState;
                    }
                    else
                    {
                        //Debug.Log("rightBumperState = " + rightBumperSensor.collisionState);
                        result = rightBumperSensor.collisionState;
                    }

                    break;
            }
        }

        return result;
    }

    private void GetDistanceSensorReading()
    {
        RaycastHit hit;
        if(Physics.Raycast(distanceSensor.transform.position, distanceSensor.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(distanceSensor.transform.position, distanceSensor.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            distanceSensorValue0 = hit.distance;
        }

        RaycastHit hit2;
        if (Physics.Raycast(distanceSensor.transform.position, distanceSensor.transform.TransformDirection(Vector3.forward)+ distanceSensor.transform.TransformDirection(Vector3.down), out hit2, Mathf.Infinity))
        {
            Debug.DrawRay(distanceSensor.transform.position, distanceSensor.transform.TransformDirection(Vector3.forward) + distanceSensor.transform.TransformDirection(Vector3.down) * hit2.distance, Color.yellow);
            distanceSensorValue45 = hit2.distance;
        }
    }
}
