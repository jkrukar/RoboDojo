﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class BlockParser : Singleton<BlockParser>
{

    private string logPrefix = "[BlockParser] ";
    private IQBlockFile blockFile;
    private XmlDocument fileXML;
    XmlNamespaceManager nsManager;
    List<Block> topBlocks = new List<Block>();
    public Stack<Block> blockStack = new Stack<Block>();

    //Variable maps
    public Dictionary<string, bool> boolVariables = new Dictionary<string, bool>();
    public Dictionary<string, float> floatVariables = new Dictionary<string, float>();
    public Dictionary<string, float[]> floatArrayVariables = new Dictionary<string, float[]>();
    public Dictionary<string, float[,]> float2dArrayVariables = new Dictionary<string, float[,]>();

    int controllerReadyCounter = 0;

    public GameObject bot;
    public Rigidbody botRigidBody; 

    void Awake()
    {
        BuildBlocksFromFile("Assets/Resources/waitTime.iqblocks");

        foreach(Block block in topBlocks) //Top blocks are always event blocks
        {
            EventsController.instance.ExecuteBlock(block);
        }

        bot = GameObject.FindGameObjectWithTag("Bot");
        botRigidBody = bot.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReceiveControllerReadySignal()
    {
        controllerReadyCounter++;

        if(controllerReadyCounter == 7)
        {
            controllerReadyCounter = 0;
            ExecuteStack();
        }
    }

    public void ExecuteStack()
    {
        //Debug.Log("blockStack count = " + blockStack.Count);

        Stack<Block> executionStack = blockStack;
        blockStack = new Stack<Block>();

        //Debug.Log("blockStack count = " + blockStack.Count);
        //Debug.Log("executionStack count = " + executionStack.Count);

        while (executionStack.Count > 0)
        {
            Block nextBlock = executionStack.Pop();
            ExecuteBlock(nextBlock);
        }
    }

    private void ExecuteBlock(Block block)
    {
        if (block.type.Contains("iq_variables_"))
        {
            VariablesController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_drivetrain_") || block.type.Contains("iq_motion_"))
        {
            DrivetrainController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_looks_"))
        {
            LooksController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_sounds_"))
        {
            SoundController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_events_"))
        {
            EventsController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_control_"))
        {
            ControlController.instance.ExecuteBlock(block);
        }
        else if (block.type.Contains("iq_sensing_"))
        {
            SensingController.instance.ExecuteBlock(block);
        }
    }

    //Builds all the blocks. Blocks form a linked list with topBlock at the head
    private void BuildBlocksFromFile(string filename)
    {
        blockFile = LoadIQBlockFile(filename);
        fileXML = LoadXMLfromText(blockFile.wrkspace);
        nsManager = new XmlNamespaceManager(fileXML.NameTable);
        nsManager.AddNamespace("d", fileXML.DocumentElement.NamespaceURI);

        XmlNode root = fileXML.FirstChild;
        XmlNode variables = null;
        List<XmlNode> topBlockElements = new List<XmlNode>();

        if (root.HasChildNodes)
        {
            //Separate variables from top level block
            foreach (XmlNode child in root.ChildNodes)
            {
                string name = child.Name;
                //Debug.Log(logPrefix + "child= " + name);

                switch (name)
                {
                    case "block":
                        //Debug.Log(logPrefix + " Found Top Level Block!");
                        topBlockElements.Add(child);
                        break;
                    case "variables":
                        //Debug.Log(logPrefix + " Found Variables");
                        variables = child;
                        break;
                    default:
                        Debug.LogError(logPrefix + "Missed Node of Type: " + name);
                        break;
                }
            }

            //Store variables
            StoreGlobalVariables(variables);

            //Build blocks from the top block elements
            foreach(XmlNode topBlockElement in topBlockElements)
            {
                topBlocks.Add(BuildBlock(topBlockElement));
            }            
        }

        //Print out block stacks
        foreach(Block topBlock in topBlocks)
        {
            bool traverseBlocks = true;
            Block currentBlock = topBlock;

            Debug.Log(logPrefix + " Traverse Blocks:");

            while (traverseBlocks)
            {
                Debug.Log(logPrefix + currentBlock.type);

                if (currentBlock.nextBlock != null)
                {
                    currentBlock = currentBlock.nextBlock;
                }
                else
                {
                    traverseBlocks = false;
                }
            }
        }        
    }



    //TODO missing <mutation> tag like in variables



    //Stores the global variables from the <variables> tag
    private void StoreGlobalVariables(XmlNode variablesNode)
    {
        XmlNodeList variableNodes = variablesNode.SelectNodes("child::d:variable", nsManager);

        foreach(XmlNode variableNode in variableNodes)
        {
            XmlAttributeCollection attributes = variableNode.Attributes;
            string variableName = variableNode.InnerText;
            string type = attributes["type"].InnerText;
            int length = int.Parse(attributes["arraylength"].InnerText);
            int width = int.Parse(attributes["arraywidth"].InnerText);

            Debug.Log(logPrefix + " store variable: " + variableName + " of type " + type);

            switch (type)
            {
                case "":
                    //Debug.Log(logPrefix + " stored int var");
                    floatVariables.Add(variableName, 0);
                    break;
                case "boolean":
                    boolVariables.Add(variableName, false);
                    //Debug.Log(logPrefix + " stored bool var");
                    break;
                case "list":
                    //Debug.Log(logPrefix + " stored list var of length " + length);
                    float[] newArray = new float[length];
                    floatArrayVariables.Add(variableName, newArray);
                    break;
                case "array2d":                    
                    //Debug.Log(logPrefix + " stored 2D list var of size " + length + "x" + width);
                    float[,] new2dArray = new float[length,width];
                    float2dArrayVariables.Add(variableName, new2dArray);
                    break;
            }
        }
    }

    //Builds a BlockValue from a <statement> element
    private BlockStatement BuildBlockStatement(XmlNode node)
    {
        BlockStatement newStatement = new BlockStatement();

        XmlAttributeCollection attributes = node.Attributes;
        XmlNodeList blocks = node.SelectNodes("child::d:block", nsManager);

        if (blocks.Count > 1)
        {
            Debug.LogError("ERROR: MORE ELEMENTS THAN EXPECTED WHEN BUILDING BLOCK SHADOW!!!");
        }

        if (attributes["name"] != null)
            newStatement.name = attributes["name"].InnerText;

        if (blocks != null && blocks.Count > 0)
        {
            Block newBlock = BuildBlock(blocks.Item(0));
            newStatement.block = newBlock;
        }

        Debug.Log(logPrefix + "New Statement: " + newStatement.name);

        return newStatement;
    }

    //Builds a BlockValue from a <shadow> element
    private BlockShadow BuildBlockShadow(XmlNode node)
    {
        BlockShadow newShadow = new BlockShadow();

        XmlAttributeCollection attributes = node.Attributes;
        XmlNodeList fields = node.SelectNodes("child::d:field", nsManager);
        XmlNodeList values = node.SelectNodes("child::d:value", nsManager);

        if (attributes["type"] != null)
            newShadow.type = attributes["type"].InnerText;

        if (fields.Count > 1)
        {
            Debug.LogError("ERROR: MORE ELEMENTS THAN EXPECTED WHEN BUILDING BLOCK SHADOW!!!");
        }

        if (fields != null && fields.Count > 0)
        {
            BlockField newField = BuildBlockField(fields.Item(0));
            newShadow.field = newField;
        }

        if(values != null && values.Count > 0)
        {
            foreach (XmlNode nextValueElement in values)
            {
                BlockValue newValue = BuildBlockValue(nextValueElement);
                newShadow.values.Add(newValue);
            }
        }

        Debug.Log(logPrefix + "New Shadow: " + newShadow.type);

        return newShadow;
    }

    //Builds a BlockValue from a <value> element
    private BlockValue BuildBlockValue(XmlNode node)
    {
        BlockValue newValue = new BlockValue();

        XmlAttributeCollection attributes = node.Attributes;
        XmlNodeList shadows = node.SelectNodes("child::d:shadow", nsManager);
        XmlNodeList fields = node.SelectNodes("child::d:field", nsManager);
        XmlNodeList blocks = node.SelectNodes("child::d:block", nsManager);

        if(shadows.Count > 1 || fields.Count > 1 || blocks.Count > 1)
        {
            Debug.LogError("ERROR: MORE ELEMENTS THAN EXPECTED WHEN BUILDING BLOCK VALUE!!!");
        }

        if (attributes["name"] != null)
            newValue.name = attributes["name"].InnerText;

        if (shadows != null && shadows.Count>0)
        {
            BlockShadow newShadow = BuildBlockShadow(shadows.Item(0));
            newValue.shadow = newShadow;
        }

        if (fields != null && fields.Count > 0)
        {
            Debug.Log("Fields count = " + fields.Count);
            BlockField newField = BuildBlockField(fields.Item(0));
            newValue.field = newField;
        }

        if (blocks != null && blocks.Count > 0)
        {
            Block newBlock = BuildBlock(blocks.Item(0));
            newValue.block = newBlock;
        }

        Debug.Log(logPrefix + "New Value: " + newValue.name);

        return newValue;
    }

    //Builds a BlockField from a <field> element
    private BlockField BuildBlockField(XmlNode node)
    {
        BlockField newField = new BlockField();

        XmlAttributeCollection attributes = node.Attributes;

        newField.value = node.InnerText;

        if(attributes["name"] != null)
            newField.name = attributes["name"].InnerText;

        if (attributes["variabletype"] != null)
            newField.variabletype = attributes["variabletype"].InnerText;

        Debug.Log(logPrefix + "New Field: " + newField.name + " : " + newField.variabletype + " : " + newField.value);

        return newField;
    }

    //Builds a Block from a <block> element
    private Block BuildBlock(XmlNode blockElement)
    {
        Block newBlock = new Block();

        XmlAttributeCollection attributes = blockElement.Attributes;
        XmlNodeList values = blockElement.SelectNodes("child::d:value", nsManager);
        XmlNodeList fields = blockElement.SelectNodes("child::d:field", nsManager);
        XmlNodeList statements = blockElement.SelectNodes("child::d:statement", nsManager);
        XmlNodeList nexts = blockElement.SelectNodes("child::d:next",nsManager);

        newBlock.type = attributes["type"].InnerText;
        Debug.Log(logPrefix + " made new block of type: " + newBlock.type);

        if (values != null)
        {
            foreach(XmlNode nextValueElement in values){
                BlockValue newValue = BuildBlockValue(nextValueElement);
                newBlock.values.Add(newValue);
            }            
        }

        if(fields != null)
        {
            foreach (XmlNode nextFieldElement in fields)
            {
                BlockField newField = BuildBlockField(nextFieldElement);
                newBlock.fields.Add(newField);
            }
        }

        if (statements != null)
        {
            foreach (XmlNode nextStatementElement in statements)
            {
                BlockStatement newStatement = BuildBlockStatement(nextStatementElement);
                newBlock.statements.Add(newStatement);
            }
        }

        if (nexts != null)
        {
            foreach (XmlNode nextElement in nexts)
            {
                newBlock.nextBlock = (BuildBlock(nextElement.FirstChild)); //A block is always the only child of a <next> element
            }
        }

        return newBlock;
    }

    //Loads the xml doc from the wrkspace of an IQBlockFile
    private XmlDocument LoadXMLfromText(string xmlText)
    {
        Debug.Log(logPrefix + "XML: " + xmlText);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);
        return xmlDoc;
    }

    //Loads the .iqblocks file and builds an IQBlockFile object with the content
    private IQBlockFile LoadIQBlockFile(string filename)
    {
        //Debug.Log(logPrefix + "loading: " + filename);

        string fileText = File.ReadAllText(filename);
        IQBlockFile file = JsonUtility.FromJson<IQBlockFile>(fileText);

        return file;
    }
}
