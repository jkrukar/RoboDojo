using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    public Material deactiveMaterial;
    public Material activeMaterial;

    public Trigger trigger;
    public List<Vector3> wayPoints;
    private Vector3 targetPosition;
    private int wayPointIndex = 0;
    public float speed = 2;
    public float loiterTime = 1;
    public bool active = false;
    private bool loitering = false;

    // Start is called before the first frame update
    void Start()
    {
        trigger.triggerEvent.AddListener(OnTrigger);

        wayPoints.Insert(0,transform.position);
        targetPosition = wayPoints[0];
    }

    // Update is called once per frame
    void Update()
    {
   
        if (!loitering && active && wayPoints.Count > 1)
        {
            float remainingDistance = Vector3.Distance(transform.position, targetPosition);

            if (remainingDistance > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            }
            else if(remainingDistance < 0.01)
            {
                wayPointIndex++;

                if (wayPointIndex > wayPoints.Count-1)
                {
                    wayPointIndex = 0;
                }

                targetPosition = wayPoints[wayPointIndex];

                StartCoroutine(Loiter());
            }
        }
    }

    private IEnumerator Loiter()
    {
        loitering = true; 

        yield return new WaitForSeconds(loiterTime);

        loitering = false;
    }

    public void OnTrigger()
    {
        active = !active;

        Debug.Log("Set Moving Block Active = " + active);

        if (active)
        {
            gameObject.GetComponent<MeshRenderer>().material = activeMaterial;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = deactiveMaterial;
        }

    }
}
