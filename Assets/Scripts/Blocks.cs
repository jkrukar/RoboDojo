using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Block
{
    public string type;
    public List<BlockField> fields = new List<BlockField>();
    public List<BlockValue> values = new List<BlockValue>();
    public List<BlockStatement> statements = new List<BlockStatement>();
    public Block nextBlock;
    public bool finished = false;
}

public class BlockField
{
    public string name;
    public string value;
    public string variabletype;
}

public class BlockValue
{
    public string name;
    //shadows multiple? multiple fields?
    public BlockShadow shadow;
    public BlockField field;
    public Block block;

}

public class BlockStatement
{
    public Block block;
    public string name;
    public bool finished = false;

}

public class BlockShadow
{
    public string type;
    public BlockField field;
    public List<BlockValue> values = new List<BlockValue>();
}

public class BlockStack
{
    public Block startBlock;
    public BlockStack returnStack;
    public bool active = false;
    public bool finished = false;

    public BlockStack(Block block, BlockStack stack)
    {
        startBlock = block;
        returnStack = stack;
    }
}