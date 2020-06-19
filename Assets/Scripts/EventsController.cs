using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsController : Singleton<EventsController>
{
    private string logPrefix = "[Events] ";
    public float timer = 0.0f;
    List<BlockStack> whenStartedStacks = new List<BlockStack>();
    Dictionary<float,BlockStack> whenTimerStacks = new Dictionary<float,BlockStack>();
    Dictionary<string,BlockStack> whenBroadcastedStacks = new Dictionary<string,BlockStack>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("whenStartedStacks:");
        foreach (BlockStack stack in whenStartedStacks)
        {
            Debug.Log("\tnext:" + stack.startBlock.type);
        }

        Debug.Log("whenTimerStacks:");
        foreach(KeyValuePair<float,BlockStack> kvp in whenTimerStacks)
        {
            Debug.Log("\tnext:" + kvp.Key + " : " + kvp.Value.startBlock.type);
        }

        Debug.Log("whenBroadcastedStacks:");
        foreach (KeyValuePair<string, BlockStack> kvp in whenBroadcastedStacks)
        {
            Debug.Log("\tnext:" + kvp.Key + " : " + kvp.Value.startBlock.type);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

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
                float triggerTime = BlockParser.instance.ResolveBlockValue(block.values[0]);
                whenTimerStacks.Add(triggerTime,new BlockStack(block, null));
                break;
            case "iq_events_when_started":
                BlockStack stack = new BlockStack(block, null);
                stack.active = true; //TODO: does nothing
                BlockParser.instance.blockStack.Push(stack.startBlock.nextBlock);
                whenStartedStacks.Add(stack);

                break;
            case "iq_events_when_broadcasted":
                string message = block.fields[0].value; //This kind of block only has 1 field so this is safe.
                whenBroadcastedStacks.Add(message,new BlockStack(block, null));
                break;
        }
    }
}
