using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlController : Singleton<ControlController>
{
    private string logPrefix = "[Control] ";
    private Block activeBlock = null;
    private bool gameRunning = true;

    Stack<BlockStatement> statementStack = new Stack<BlockStatement>();

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
            case "iq_control_break":  ///Handled implicitly in ExecuteStatement, by terminating if a break block is encountered
                block.finished = true;
                break;
            case "iq_control_false_head":
                block.finished = true;
                break;
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

        while (gameRunning && !statement.brokenOut)
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
        statement.brokenOut = false;

        Debug.Log("CP1");

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

        while (condition && !statement.brokenOut)
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
        statement.brokenOut = false;

        yield return null;
    }

    private IEnumerator RepeatUntil(Block block)
    {
        bool condition = BlockParser.instance.ResolveBlockCondition(block.values[0]);

        BlockStatement statement = block.statements[0];
        statement.finished = true; //This will get the statement started, although it's no technically finished

        while (!condition && !statement.brokenOut)
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
        statement.brokenOut = false;

        yield return null;
    }

    private IEnumerator RepeatFor(Block block)
    {
        int iterations = (int) BlockParser.instance.ResolveBlockValue(block.values[0]);
        BlockStatement statement = block.statements[0];

        statement.finished = true; //This will get the statement started, although it's no technically finished

        while (iterations > 0 && !statement.brokenOut)
        {
            iterations--;
            statement.finished = false; //reset statement
            yield return StartCoroutine(ExecuteStatement(statement));
        }
        
        activeBlock = block; //Reset activeBlock after executing statement blocks
        block.finished = true;
        statement.brokenOut = false;

        yield return null;
    }

    private IEnumerator ExecuteStatement(BlockStatement statement)
    {
        Block nextStatementBlock = statement.block;
        Block finalStatementBlock = nextStatementBlock;

        statementStack.Push(statement);

        while(finalStatementBlock.nextBlock != null)
        {
            finalStatementBlock.statementBlock = true;
            finalStatementBlock = finalStatementBlock.nextBlock;
        }

        Debug.Log("final statement block = " + finalStatementBlock.type);

        Debug.Log("init statement block = " + nextStatementBlock.type);

        //Push first block in statement
        Block statementHead = new Block();
        statementHead.type = "iq_control_false_head";
        statementHead.nextBlock = nextStatementBlock;
        nextStatementBlock = statementHead;

        BlockParser.instance.blockStack.Push(nextStatementBlock); //Create a null block as the head to kick things off - mark as finished right away

        Debug.Log("Execute Statement: " + statement.parentBlock.type);

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

                    //Debug.Log("next statement block = " + nextStatementBlock.type);

                    if(nextStatementBlock.type.Contains("iq_control_break")) //If the next block is a break, terminate executing the statement immediately
                    {
                        //Debug.Log("statement stack size = " + statementStack.Count);
                        //Pop blocks off the statement stack

                        bool doneBreaking = false;

                        while (!doneBreaking)
                        {
                            BlockStatement nextStatement = statementStack.Pop();

                            //Debug.Log("Set " + nextStatement.parentBlock.type + " to finished");
                            nextStatement.finished = true;                         

                            if (!nextStatement.parentBlock.type.Contains("if_then"))
                            {
                                nextStatement.brokenOut = true;
                                doneBreaking = true;
                            }
                            
                            //Debug.Log("next block in statement stack = " + nextStatement.parentBlock.type);
                        }

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

        if(statementStack.Count > 0 && statementStack.Peek() == statement) //If this statement is at the top of the stack, pop it off to finish
        {
            statementStack.Pop();
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
