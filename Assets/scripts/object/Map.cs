using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map
{
    [System.Serializable]
    public struct threatNode
    {
        public string title;
        public string desc;
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

    public string filed;
    public List<mapNode> regen;
    public List<threatNode> threat;

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
