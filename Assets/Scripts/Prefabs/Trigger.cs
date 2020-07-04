using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public UnityEvent triggerEvent = new UnityEvent();

    public enum TriggerTypes {Proximity, Collision};
    public TriggerTypes triggerType;

    public GameObject triggerObject;
    public float resetTime = 0f;
    private bool resetting = false;

    private Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = gameObject.GetComponent<Collider>();

        if(collider == null)
        {
            Debug.Log("Did not have collider so added one!");
            collider = gameObject.AddComponent<BoxCollider>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!resetting && triggerType == TriggerTypes.Collision && collision.collider.gameObject.transform.IsChildOf(triggerObject.transform))
        {
            resetting = true;
            collider.enabled = false;
            TriggerEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (resetting)
        {
            StartCoroutine(ResetTrigger());
        }
    }

    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(resetTime);
        resetting = false;
    }

    private void TriggerEvent()
    {
        triggerEvent.Invoke();
    }
}
