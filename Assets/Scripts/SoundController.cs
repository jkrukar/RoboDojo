using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : Singleton<SoundController>
{
    private string logPrefix = "[Sound] ";
    private Block activeBlock;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBlock != null)
        {
            if (activeBlock.finished && !activeBlock.statementBlock)
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
            case "iq_sounds_play_sound":
                break;
            case "iq_sounds_play_note":
                break;
        }

        activeBlock.finished = true;
    }
}
