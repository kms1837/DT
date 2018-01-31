using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoard : MonoBehaviour {
    public GameObject BottomView;
    public GameObject MainUI;
    public GameObject CreateUI;
    public GameObject CreatePopup;
    public GameObject RoomUI;

    public GameObject SelectButton; // 선택용 버튼(복제용)

    private int currentUI; // 현재 UI
    private int prevUI; // 이전 UI

    private int currentStatus; // UI 상태


    private enum UIStatus { Main, Create, Room };
    private enum CreateStatus { Place, Gola, Monster, Skill };

    private Dictionary<string, object> room = new Dictionary<string, object>();
    private ArrayList dummy = new ArrayList();
    private string[] dummySetName = new string[] { "목적지", "목표", "수렵", "스킬선택" };
    private string[] dummySkillNameList = new string[] { "강한 범위 공격", "강한 단일 공격", "단일 메즈", "범위 메즈", "순간 회복", "피해 감소" };
    private string[] dummyPlaceList = new string[] { "A숲", "B숲", "C숲", "D숲", "E숲", "F숲", "G숲" };
    private string[] dummyGolaList = new string[] { "수렵", "채집", "횡단", "탐험" };

    // 추후 통합
    private string[] dummyMonsterList = new string[] { "늑대왕", "거대곰", "등등", "이것" };
    private string[] dummyMonsterInfoList = new string[] { "lv1권장", "정말 거대한 곰입니다.", "테스트", "test" };
    //


    void Start () {
        currentUI = (int)UIStatus.Main;
        prevUI = (int)UIStatus.Main;
        homeBtnAction();

        room.Add("skill", new ArrayList()); // skills

        dummy.Add(dummyPlaceList);
        dummy.Add(dummyGolaList);
        dummy.Add(dummyMonsterList);
        dummy.Add(dummySkillNameList);
        /*
         {
            title: "목적",
            key: "Place",
            items: []
         }

         {
            target: "늑대왕",
            level: 1
         }

         {
            name: "순간 회복",
            icon: "img/healing",
         }
         */
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void changeUI() {
        prevUI = currentUI;
        hiddenUI();
    }

    public void partyBackBtnAction() {
        Text titleLabel = CreateUI.transform.Find("TitleLabel").GetComponent<Text>();

        currentStatus = !CreatePopup.activeSelf? currentStatus - 1 : currentStatus;
        CreatePopup.SetActive(false);
        CreateUI.transform.Find("SkillSet").gameObject.SetActive(false);

        ArrayList setSkills = (ArrayList)room["skill"];
        setSkills.Clear();

        if (currentStatus < 0) {
            currentStatus = 0;
            homeBtnAction();
        } else {
            partySelectBtnBatch(dummy[currentStatus] as string[]);
        }

        string key = Enum.GetName(typeof(CreateStatus), currentStatus);
        titleLabel.text = (currentStatus == (int)CreateStatus.Monster) ? dummyGolaList[(int)room[key]] : dummySetName[currentStatus];
    }

    public void partyCreateComplateBtn() {
        changeUI();
        currentUI = (int)UIStatus.Room;
        CreateUI.SetActive(false);
        RoomUI.SetActive(true);
    }

    public void partySelectBtnAction(int selectNum) {
        Text titleLabel = CreateUI.transform.Find("TitleLabel").GetComponent<Text>();
        Text skillListLabel = CreateUI.transform.Find("SkillSet").Find("SkillListLabel").GetComponent<Text>();
        skillListLabel.text = "";

        switch (currentStatus) {
            case (int)CreateStatus.Monster:
                if (!CreatePopup.activeSelf) {
                    CreatePopup.SetActive(true);
                    Text popupTitle = CreatePopup.transform.Find("TitleLabel").GetComponent<Text>();
                    Text popupDescription = CreatePopup.transform.Find("DescriptionLabel").GetComponent<Text>(); 

                    popupTitle.text = dummyMonsterList[selectNum];
                    popupDescription.text = dummyMonsterInfoList[selectNum];
                    room["monster"] = selectNum;
                    CreatePopup.transform.SetSiblingIndex(CreateUI.transform.childCount);
                } else {
                    currentStatus = (int)CreateStatus.Skill;
                    CreatePopup.SetActive(false);
                    CreateUI.transform.Find("SkillSet").gameObject.SetActive(true);
                    //partySkillBtnBatch(dummy[currentStatus] as string[]);
                    partySelectBtnBatch(dummy[currentStatus] as string[]);
                    titleLabel.text = "스킬세팅";
                    // 교체예정
                }
                break;
            case (int)CreateStatus.Skill:
                currentStatus = (int)CreateStatus.Skill;
                ArrayList setSkills = (ArrayList)room["skill"];
                setSkills.Add(selectNum);
                foreach (int skillNum in setSkills) {
                    string skillName = dummySkillNameList[skillNum];
                    skillListLabel.text += skillName + " ";
                }
                break;
            default:
                string key = Enum.GetName(typeof(CreateStatus), currentStatus);
                room[key] = selectNum;
                titleLabel.text = (currentStatus == (int)CreateStatus.Gola) ? dummyGolaList[selectNum] : dummySetName[currentStatus + 1];
                currentStatus++;
                currentStatus = currentStatus % (Enum.GetNames(typeof(CreateStatus)).Length); // overflow 방지
                partySelectBtnBatch(dummy[currentStatus] as string[]);
                break;
        }
    }

    private void partySkillBtnBatch(string[] btnNames) {
        Transform selectBtnGroup = CreateUI.transform.Find("SelectBtnGroup");
        if (selectBtnGroup != null) {
            Destroy(selectBtnGroup.gameObject);
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

        int count = 0;
        foreach (string name in btnNames) {
            Transform selectBtn = Instantiate(SelectButton).transform;
            selectBtn.GetChild(0).GetComponent<Text>().text = name;
            selectBtn.SetParent(selectBtnGroup, false);
            selectBtn.localPosition = new Vector2(left, top);

            int index = count;
            selectBtn.GetComponent<Button>().onClick.AddListener(() => { partySelectBtnAction(index); });

            left += btnRect.width + margin;
            count++;

            if (left > newLinePoint) {
                int batchLength = btnNames.Length - count;
                left = batchLength >= maxBtnNumber ? leftStandard : -(((btnRect.width + margin) * batchLength) / 2) + (btnRect.width / 2);
                top -= btnRect.height + margin;
            }
        }
    }

    public void partyCreateBtnAction() {
        changeUI();
        CreateUI.SetActive(true);
        currentUI = (int)UIStatus.Create;

        partySelectBtnBatch(dummy[0] as string[]);
        currentStatus = (int)CreateStatus.Place;
    }

    public void partyJoinBtnAction() {
        changeUI();
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
        if (currentUI == (int)UIStatus.Create) {
            partyBackBtnAction();
        } else {
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
}
