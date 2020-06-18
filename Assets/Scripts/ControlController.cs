using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlController : Singleton<ControlController>
{
    private string logPrefix = "[Control] ";
    private Block activeBlock = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBlock != null)
        {
            if (activeBlock.finished)
            {
                if (activeBlock.nextBlock != null)
                {
                    //Debug.Log("Push block: " + activeBlock.nextBlock.type);
                    BlockParser.instance.blockStack.Push(activeBlock.nextBlock);
                }

                activeBlock = null;
            }
        }

        BlockParser.instance.ReceiveControllerReadySignal();
    }

    public void ExecuteBlock(Block block)
    {
        Debug.Log(logPrefix + "execute block of type: " + block.type);
        activeBlock = block;

        switch (block.type)
        {
            case "iq_control_wait":
                StartCoroutine(WaitForSeconds(block));
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

    private IEnumerator WaitForSeconds(Block block)
    {
        float waitTime = float.Parse(block.values[0].shadow.field.value);

        Debug.Log("Start waiting for " + waitTime + " seconds");

        yield return new WaitForSeconds(waitTime);

        block.finished = true;
        Debug.Log("Done waiting!");

        yield return null;
    }
}
