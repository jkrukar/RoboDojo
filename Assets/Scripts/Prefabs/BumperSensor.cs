using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperSensor : MonoBehaviour
{
    public bool collisionState = false; 

    private void OnTriggerEnter(Collider collider)
    {
        collisionState = true;
    }

    private void OnTriggerExit(Collider collider)
    {
        collisionState = false;
    }
}
