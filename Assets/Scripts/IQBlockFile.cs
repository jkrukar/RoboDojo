using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IQBlockFile
{
    public string wrkspace;
    public List<RConfig> rconfig;
    public int slot;
    public string cpp;
    public string cppStatus;
    public string platform;
    public string sdkVersion;
    public string appVersion;
    public string fileFormat;
    public string icon;
}


[System.Serializable]
public class RConfig
{
    public List<int> port;
    public bool customName;
    public string deviceType;
    public string name;
    public Setting setting;
}

[System.Serializable]
public class Setting
{
    public string type;
    public string wheelSize;
    public string gearRatio;
    public string direction;
    public string gyroType;
    public string width;
    public string unit;
    public string wheelbase;
    public string wheelbaseUnit;
}
