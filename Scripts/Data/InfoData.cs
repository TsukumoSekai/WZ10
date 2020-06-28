using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public static class InfoDataCenter
{
    private static InfoData infoData;
    public static InfoData InfoData
    {
        get
        {
            if (infoData == null)
                infoData = GetInfoData();
            return infoData;
        }
    }

    public static InfoData GetInfoData()
    {
        string jsonString = File.ReadAllText(Application.streamingAssetsPath + "/JsonData/InfoData.json");
        return JsonUtility.FromJson<InfoData>(jsonString);
    }
}

[Serializable]
public class InfoData
{
    public List<Info> InfoList = new List<Info>();
}

[Serializable]
public class Info
{
    public string Title;
    public string Content;
}