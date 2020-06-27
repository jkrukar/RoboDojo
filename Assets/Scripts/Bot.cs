using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : Singleton<Bot>
{
    public int coinCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CollectCoin(int value)
    {
        coinCount += value;
        Debug.Log("coinCount = " + coinCount);
    }
}
