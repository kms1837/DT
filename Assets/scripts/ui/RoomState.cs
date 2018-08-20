using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomState
{
    [System.Serializable]
    public struct roomData
    {
        public string[] setNames;
        public string[] golaList;
        public string[] skillSpecNameList;
    }

    public static int playerID; // 플레이어 id(저장소 변경할것)
    public static int orderUser; // 지휘유저 id
    public static int place; // 장소 id
    public static int gola; // 목표
    public static int threat; // 위협 id
    public static ArrayList skills; // 설정 스킬들
    public static ArrayList users; // 참여 유저들

    public static void addUser(int userID) {
        if (users == null) {
            users = new ArrayList();
        }

        users.Add(userID);
    }
    
    public static roomData loadRoomData(string fileName) {
        roomData? loadedRoomData = null;

        try {
            TextAsset jsonText = Resources.Load(fileName) as TextAsset;
            loadedRoomData = JsonUtility.FromJson<roomData>(jsonText.text);
        }
        catch (System.Exception error) {
            Debug.LogError(error);
        }

        return (roomData)loadedRoomData;
    }
}
