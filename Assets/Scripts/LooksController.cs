using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooksController : Singleton<LooksController>
{
    private string logPrefix = "[LooksController] ";
    private Block activeBlock = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBlock != null && !activeBlock.statementBlock)
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
            case "iq_looks_print":
                break;
            case "iq_looks_set_cursor":
                break;
            case "iq_looks_next_row":
                break;
            case "iq_looks_set_print_precision":
                break;
            case "iq_looks_clear_all_rows":
                break;
            case "iq_looks_clear_row":
                break;
        }

        block.finished = true;
    }
}
