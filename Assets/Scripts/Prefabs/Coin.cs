using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public static float rotationSpeed = 120;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Rotate(Vector3.forward, Random.Range(0f,180f));
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bot")
        {
            Bot.instance.CollectCoin(value);
            Destroy(gameObject);
        }
    }
}
