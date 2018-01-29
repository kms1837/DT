using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
class RoomState {
    public string key;
    public 
}*/

public class QuestBoard : MonoBehaviour {
    public GameObject BottomView;
    public GameObject MainUI;
    public GameObject CreateUI;
    public GameObject RoomUI;

    public GameObject SelectButton; // 선택용 버튼(복제용)

    private int currentUI; // 현재 UI
    private int prevUI; // 이전 UI

    private int currentStatus; // UI 상태

    private enum UIStatus { Main, Create, Room };
    private enum CreateStatus { Place, Gola, Monster }

    private ArrayList dummy = new ArrayList();

    // Use this for initialization
    void Start () {
        currentUI = (int)UIStatus.Main;
        prevUI = (int)UIStatus.Main;
        homeBtnAction();

        dummy.Add(new string[] { "A숲", "B숲", "C숲", "D숲", "E숲", "F숲", "G숲" });
        dummy.Add(new string[] { "수렵", "채집", "횡단", "탐험" });
        dummy.Add(new string[] { "늑대왕", "거대곰", "등등", "이것" });
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void changeUI() {
        prevUI = currentUI;
    }

    public void partySelectBtnAction() {
        currentStatus++;
        if (currentStatus >= Enum.GetNames(typeof(CreateStatus)).Length) {
            // 셋팅 완료
            changeUI();
            currentUI = (int)UIStatus.Room;
            CreateUI.SetActive(false);
            RoomUI.SetActive(true);
        } else {
            partySelectBtnBatch(dummy[currentStatus] as string[]);
        }
    }

    private void partySelectBtnBatch(string[] btnNames) {
        Transform selectBtnGroup = CreateUI.transform.Find("SelectBtnGroup");
        if (selectBtnGroup != null) {
            Destroy(selectBtnGroup.gameObject);
        }

        Rect viewRect = BottomView.transform.GetComponent<RectTransform>().rect;
        Rect btnRect = SelectButton.transform.GetComponent<RectTransform>().rect;
        int maxBtnNumber = 3; // 한라인에 배치할 버튼 갯수
        float margin = 20;
        float groupWidth = (btnRect.width + margin) * maxBtnNumber;

        selectBtnGroup = new GameObject("SelectBtnGroup", typeof(RectTransform)).transform;
        RectTransform groupRect = selectBtnGroup.GetComponent<RectTransform>();

        selectBtnGroup.SetParent(CreateUI.transform, false);
        selectBtnGroup.localPosition = new Vector2(0, 0);
        groupRect.sizeDelta = new Vector2(groupWidth, 0);

        float leftStandard = -(groupWidth / 2) + (btnRect.width/2);
        float newLinePoint = (groupWidth / 2) - (btnRect.width / 2); //viewRect.width - leftStandard;

        float left = leftStandard;
        float top = 0;

        int batchLength = btnNames.Length;
        foreach (string name in btnNames) {
            Transform selectBtn = Instantiate(SelectButton).transform;
            selectBtn.GetChild(0).GetComponent<Text>().text = name;
            selectBtn.SetParent(selectBtnGroup, false);
            selectBtn.localPosition = new Vector2(left, top);

            selectBtn.GetComponent<Button>().onClick.AddListener(partySelectBtnAction);

            left += btnRect.width + margin;
            batchLength--;

            if (left > newLinePoint) {
                left = batchLength >= maxBtnNumber ? leftStandard : -(((btnRect.width + margin) * batchLength) / 2) + (btnRect.width / 2);
                top -= btnRect.height + margin;
            }
        }
    }

    public void partyCreateBtnAction() {
        changeUI();
        hiddenUI();
        CreateUI.SetActive(true);
        currentUI = (int)UIStatus.Create;

        partySelectBtnBatch(dummy[0] as string[]);
        currentStatus = (int)CreateStatus.Place;
    }

    public void partyJoinBtnAction() {
        changeUI();
        hiddenUI();
    }

    public void homeBtnAction() {
        hiddenUI();
        MainUI.SetActive(true);
        currentUI = (int)UIStatus.Main;
    }

    private void hiddenUI() {
        CreateUI.SetActive(false);
        RoomUI.SetActive(false);
        MainUI.SetActive(false);
    } // UI를 모두 숨깁니다.

    public void prveBtnAction() {
        switch (prevUI) {
            case (int)UIStatus.Main:
                homeBtnAction();
                break;
            case (int)UIStatus.Create:
                partyCreateBtnAction();
                break;
        }
    }
}
