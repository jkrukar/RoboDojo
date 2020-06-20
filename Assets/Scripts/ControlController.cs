using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlController : Singleton<ControlController>
{
    private string logPrefix = "[Control] ";
    private Block activeBlock = null;
    private bool gameRunning = true;

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
            case "iq_control_repeat":
                StartCoroutine(RepeatFor(block));
                break;
            case "iq_control_if_then":
                StartCoroutine(IfThen(block));
                break;
            case "iq_control_if_then_else":
                StartCoroutine(IfThenElse(block));
                break;
            case "iq_control_wait_until":
                StartCoroutine(WaitUntil(block));
                break;
            case "iq_control_repeat_until":
                StartCoroutine(RepeatUntil(block));
                break;
            case "iq_control_while":
                StartCoroutine(While(block));
                break;
            case "iq_control_forever":
                StartCoroutine(RepeatForever(block));
                break;
            //case "iq_control_break":  Handled implicitly in ExecuteStatement, by terminating if a break block is encountered
            //    break;
        }
    }

    private IEnumerator IfThenElse(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        BlockStatement ifStatement = block.statements[0];
        BlockStatement elseStatement = block.statements[1];

        if (condition)
        {
            StartCoroutine(ExecuteStatement(ifStatement));

            while (!ifStatement.finished)
            {
                yield return null;
            }
        }
        else
        {
            StartCoroutine(ExecuteStatement(elseStatement));

            while (!elseStatement.finished)
            {
                yield return null;
            }
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator IfThen(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        BlockStatement statement = block.statements[0];

        if(condition)
        {
            StartCoroutine(ExecuteStatement(statement));

            while (!statement.finished)
            {
                yield return null;
            }
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator RepeatForever(Block block)
    {
        BlockStatement statement = block.statements[0];
        statement.finished = true; //This will get the statement started, although it's no tehcnically finished

        while (gameRunning)
        {
            if (statement.finished)
            {

                if (gameRunning)
                {
                    statement.finished = false; //reset statement
                    StartCoroutine(ExecuteStatement(statement));
                }
            }
            else
            {
                yield return null;
            }
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator WaitUntil(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        while (!condition)
        {
            condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);
            yield return null;
           
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator While(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        BlockStatement statement = block.statements[0];
        statement.finished = true; //This will get the statement started, although it's no technically finished

        while (condition)
        {
            if (statement.finished)
            {
                condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

                if (condition)
                {
                    statement.finished = false; //reset statement
                    StartCoroutine(ExecuteStatement(statement));
                }
            }
            else
            {
                yield return null;
            }
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator RepeatUntil(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        BlockStatement statement = block.statements[0];
        statement.finished = true; //This will get the statement started, although it's no technically finished

        while (!condition)
        {
            if (statement.finished)
            {
                condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

                if (!condition)
                {
                    statement.finished = false; //reset statement
                    StartCoroutine(ExecuteStatement(statement));
                }                
            }
            else
            {
                yield return null;
            }            
        }

        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator RepeatFor(Block block)
    {
        int iterations = (int) BlockParser.instance.ResolveBlockValue(block.values[0]);
        BlockStatement statement = block.statements[0];

        statement.finished = true; //This will get the statement started, although it's no technically finished

        while (iterations > 0)
        {
            iterations--;
            statement.finished = false; //reset statement
            yield return StartCoroutine(ExecuteStatement(statement));
        }
        
        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;

        yield return null;
    }

    private IEnumerator ExecuteStatement(BlockStatement statement)
    {
        Block nextStatementBlock = statement.block;

        //Push first block in statement
        BlockParser.instance.blockStack.Push(nextStatementBlock);

        while (!statement.finished)
        {

            if (nextStatementBlock.finished)
            {
                nextStatementBlock.finished = false; //Reset the statement block

                //When the block finishes, get the next block in the statement or reset to first block in statement
                if (nextStatementBlock.nextBlock == null)
                {
                    statement.finished = true;
                }
                else
                {
                    nextStatementBlock = nextStatementBlock.nextBlock;

                    if(nextStatementBlock.type == "iq_control_break") //If the next block is a break, terminate executing the statement immediately
                    {
                        statement.finished = true;
                    }
                    else
                    {
                        BlockParser.instance.blockStack.Push(nextStatementBlock);
                    }                    
                }                
            }
            else
            {
                yield return null;
            }
        }

        yield return null;
    }

    private IEnumerator WaitForSeconds(Block block)
    {
        float waitTime = BlockParser.instance.ResolveBlockValue(block.values[0]);

        Debug.Log("Start waiting for " + waitTime + " seconds");

        yield return new WaitForSeconds(waitTime);

        block.finished = true;
        Debug.Log("Done waiting!");

        yield return null;
    }
}
