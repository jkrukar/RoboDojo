using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariablesController : Singleton<VariablesController>
{
    private string logPrefix = "[Variables] ";
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
            case "iq_variables_set_boolean_variable":   //Bool
                SetBool(block);
                break;
            case "iq_variables_boolean_variable":
                GetBool(block);
                break;
            case "iq_variables_variable":   //Int
                GetFloat(block);
                break;
            case "iq_variables_set_variable":
                SetFloat(block);
                break;
            case "iq_variables_change_variable":
                ChangeFloatBy(block);
                break;
            case "iq_variables_set_array_to":   //Array
                SetArrayTo(block);
                break;
            case "iq_variables_set_item_of_array":
                SetItemOfArray(block);
                break;
            case "iq_variables_size_of_array":
                GetArraySize(block);
                break;
            case "iq_variables_item_of_array":
                GetArrayItem(block);
                break;
            case "iq_variables_set_2d_array_to":    //2D Array
                Set2DArrayTo(block);
                break;
            case "iq_variables_set_item_of_2d_array":
                SetItemOf2DArray(block);
                break;
            case "iq_variables_item_of_2d_array":
                Get2DArrayItem(block);
                break;
            case "iq_variables_length_of_2d_array":
                Get2DArraySize(block);
                break;
        }

        activeBlock.finished = true; //All variable blocks are executed in one frame
    }

    public float Get2DArrayItem(Block block)
    {
        string name = block.fields[0].value;
        int row = (int) BlockParser.instance.ResolveBlockValue(block.values[0]);
        int col = (int) BlockParser.instance.ResolveBlockValue(block.values[1]);

        Debug.Log(logPrefix + "item at " + row + "," + col + "of list " + name + " = " + BlockParser.instance.float2dArrayVariables[name][row, col]);

        return BlockParser.instance.float2dArrayVariables[name][row,col];
    }

    public int Get2DArraySize(Block block)
    {
        string name = block.fields[0].value;
        string direction = block.fields[1].value;
        int size;

        if(direction == "rows")
        {
            size = BlockParser.instance.float2dArrayVariables[name].GetLength(0);
        }
        else
        {
            size = BlockParser.instance.float2dArrayVariables[name].GetLength(1);
        }

        Debug.Log(logPrefix + "size of " + name + " " + direction + " = " + size);

        return size;
    }

    private void SetItemOf2DArray(Block block)
    {
        string name = block.fields[0].value;
        int row = (int) BlockParser.instance.ResolveBlockValue(block.values[0]);
        int col = (int) BlockParser.instance.ResolveBlockValue(block.values[1]);
        float val = BlockParser.instance.ResolveBlockValue(block.values[2]);

        BlockParser.instance.float2dArrayVariables[name][row,col] = val;

        Debug.Log(logPrefix + "Set " + name + "[" + row + "][" + col + "] = " + BlockParser.instance.float2dArrayVariables[name][row, col]);
    }

    private void Set2DArrayTo(Block block)
    {
        string name = block.fields[0].value;
        int rows = block.values.Count;
        int cols = block.values[0].shadow.values.Count;
        float[,] floats = new float[rows,cols];

        for(int i=0; i < rows; i++)
        {
            for (int j=0; j < cols; j++)
            {
                floats[i,j] = BlockParser.instance.ResolveBlockValue(block.values[i].shadow.values[j]);
                Debug.Log(logPrefix + "Set " + name + "[" + i + "][" + j + "] = " + floats[i, j]);
            }
        }

        BlockParser.instance.float2dArrayVariables[name] = floats;
    }


    //Array
    public float GetArrayItem(Block block)
    {
        string name = block.fields[0].value;
        int index = (int) BlockParser.instance.ResolveBlockValue(block.values[0]);

        return BlockParser.instance.floatArrayVariables[name][index];
    }

    public int GetArraySize(Block block)
    {
        string name = block.fields[0].value;
        return BlockParser.instance.floatArrayVariables[name].Length;
    }

    private void SetItemOfArray(Block block)
    {
        string name = block.fields[0].value;
        int index = 0;
        float val = 0f;

        foreach (BlockValue value in block.values)
        {
            if(value.shadow.field.value == "INDEX")
            {
                index = (int) BlockParser.instance.ResolveBlockValue(value);
            }
            else
            {
                val = BlockParser.instance.ResolveBlockValue(value);
            }
        }

        BlockParser.instance.floatArrayVariables[name][index] = val;

        Debug.Log(logPrefix + "Set " + name + "[" + index + "] = " + BlockParser.instance.floatArrayVariables[name][index]);
    }

    private void SetArrayTo(Block block)
    {
        string name = block.fields[0].value;
        float[] floats = new float[block.values.Count];

        for(int i=0; i < block.values.Count; i++)
        {
            floats[i] = BlockParser.instance.ResolveBlockValue(block.values[i]);
            Debug.Log(logPrefix + "Set " + name + "[" + i + "] = " + floats[i]);
        }

        BlockParser.instance.floatArrayVariables[name] = floats;
    }

    private void SetBool(Block block)
    {
        string name = block.fields[0].value;
        bool value = bool.Parse(block.values[0].shadow.field.value);

        BlockParser.instance.boolVariables[name] = value;

        Debug.Log(logPrefix + "Set bool " + name + " to " + BlockParser.instance.boolVariables[name]);
    }

    public bool GetBool(Block block)
    {
        string name = block.fields[0].value;
        return BlockParser.instance.boolVariables[name];
    }

    private void SetFloat(Block block)
    {
        string name = block.fields[0].value;
        float value = BlockParser.instance.ResolveBlockValue(block.values[0]);

        BlockParser.instance.floatVariables[name] = value;

        Debug.Log(logPrefix + "Set float " + name + " to " + BlockParser.instance.floatVariables[name]);
    }

    private void ChangeFloatBy(Block block)
    {
        string name = block.fields[0].value;
        float currentVal = BlockParser.instance.floatVariables[name];
        float amount = BlockParser.instance.ResolveBlockValue(block.values[0]);

        BlockParser.instance.floatVariables[name] = currentVal + amount;

        Debug.Log(logPrefix + "Set float " + name + " to " + BlockParser.instance.floatVariables[name]);
    }

    public float GetFloat(Block block)
    {
        string name = block.fields[0].value;
        return BlockParser.instance.floatVariables[name];
    }
}
