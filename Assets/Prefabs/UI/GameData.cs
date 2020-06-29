using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public ArenaOption[] arenaOptions;
}

[Serializable]
public class ArenaOption
{
    public string name;
    public int highscore;
    public string inputFile;
}
