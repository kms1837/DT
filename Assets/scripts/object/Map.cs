using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct threat
{
    public List<mapNode> trigger;
    public List<mapNode> result;
}

[System.Serializable]
public class mapNode
{
    public string type;
    public List<int> monsterList;
    public int count;
    public float cycle;
    public int produce;
}

[System.Serializable]
public class Map
{
    public string filed;
    public List<mapNode> regen;
    public List<threat> threat;

    public static Map loadMap(string fileName) {
        Map loadedMap = null;

        try {
            TextAsset jsonText = Resources.Load(fileName) as TextAsset;
            loadedMap = JsonUtility.FromJson<Map>(jsonText.text);
        }
        catch (System.Exception error) {
            Debug.LogError(error);
        }

        return loadedMap;
    }
}
