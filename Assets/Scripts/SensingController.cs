﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensingController : Singleton<SensingController>
{
    private string logPrefix = "[Sensing] ";
    private GameObject distanceSensor;
    public float distanceSensorValue0;
    public float distanceSensorValue45;

    // Start is called before the first frame update
    void Start()
    {
        distanceSensor = GameObject.FindGameObjectWithTag("DistanceSensor");
    }

    // Update is called once per frame
    void Update()
    {
        GetDistanceSensorReading();

        BlockParser.instance.ReceiveControllerReadySignal();
    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);

        switch (block.type)
        {
            case "iq_events_broadcast":
                break;
            case "iq_events_broadcast_and_wait":
                break;
            case "iq_events_when_timer":
                break;
            case "iq_events_when_started":
                break;
            case "iq_events_when_broadcasted":
                break;
        }
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
