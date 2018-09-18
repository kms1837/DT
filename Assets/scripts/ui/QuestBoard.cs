using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using tupleType = System.Collections.Generic.Dictionary<string, object>;


public class QuestBoard : MonoBehaviour
{
    public GameObject BottomView;
    public GameObject Popup;
    public GameObject MainUI;
    public GameObject CreateUI;
    public GameObject RoomUI;
    public Transform BlackPanel;
    public Transform SceneFaderImage;

    // 선택용 버튼(복제용)
    public GameObject SelectButton;
    public GameObject SkillSelectButton;

    // 파티 로비 정보(복제용)
    public GameObject UserPanel;

    private int currentUI; // 현재 UI 상태
    private int prevUI; // 이전 UI 상태

    private int currentStatus; // UI 상태

    private enum UIStatus { Main, Create, Room };
    private enum CreateStatus { Place, Gola, Threat, Skill, Review };

    private bool recruitmentFlag;

    // 항목당 리스트(임시)
    private Dictionary<string, object> room = new Dictionary<string, object>();

    Dictionary<int, tupleType> threatList;
    Dictionary<int, tupleType> placeList;
    Dictionary<int, tupleType> jobClassList;
    Dictionary<int, tupleType> skillList;

    private Ability mySpec; // 샘플 유저(자신)
    private ArrayList dummyUsers; // 샘플 유저들
    private ArrayList userList; // 파티 참가중인 유저들

    private ArrayList userSkillList; // 유저가 선택가능한 스킬목록

    // common constant
    private const int ButtonMargin = 10;

    // create constant
    private const int MaxSetSkill = 5; // 설정 가능한 최대 스킬수
    private const int MinSetSkill = 1;
    private const string ReviewBtnGroupName = "ReviewSelectBtnGroup";

    // room constant
    private const int MaxUser = 4;
    private const string UserPanelGrupName = "UserList";

    private DataBase db;
    private RoomState.roomData uiData;

    void Start() {
        // issue - UI별로 script분리 필요
        uiData = RoomState.loadRoomData("json/quest_board");
        currentUI = (int)UIStatus.Main;
        prevUI = (int)UIStatus.Main;
        homeBtnAction();

        room.Add("Skill", new ArrayList()); // skills

        userList = new ArrayList();

        db = new DataBase("DT");

        threatList = db.getList("threats");
        placeList = db.getList("places");
        skillList = db.getList("skills");
        jobClassList = db.getList("jobclass");

        userSkillList = new ArrayList();
        for (int i = 0; i < uiData.skillSpecNameList.Length; i++) {
            userSkillList.Add(i);
            //(string)skillList[i + 1]["name"]
        }

        mySpec = new Ability();
        mySpec.nameStr = "주인공";
        mySpec.jobClass = 1;
        mySpec.mainSkill = 2;
        room.Add("OrderUser", 1);

        dummyUsers = new ArrayList();
        dummyUsers.Add(2);
        dummyUsers.Add(3);
        dummyUsers.Add(4);
        dummyUsers.Add(5);

        recruitmentFlag = false;

        SceneFaderImage.GetComponent<Fader>().fadeOutStart(() => {
            SceneFaderImage.gameObject.SetActive(false);
        });
    }

    private void changeUI() {
        prevUI = currentUI;
        hiddenUI();
    }

    public void partyRecruitmentBtnAction() {
        if (userList.Count < MaxUser) {
            GameObject clickBtn = EventSystem.current.currentSelectedGameObject;
            Text recruitmentBtnLabel = clickBtn.transform.GetChild(0).GetComponent<Text>();
            recruitmentFlag = !recruitmentFlag;
            recruitmentBtnLabel.text = recruitmentFlag ? "모집중" : "모집하기";

            if (recruitmentFlag) {
                InvokeRepeating("partyRecruiting", 2.0f, 2.0f);
            } else {
                CancelInvoke("partyRecruiting");
            }
        }
    }

    public void partyRecruiting() {
        Transform userListUIGroup = RoomUI.transform.Find(UserPanelGrupName);
        Transform displayPanel = userListUIGroup.GetChild(userList.Count);
        Transform displayInfo = displayPanel.GetChild(1);

        int userID = (int)dummyUsers[userList.Count];
        userList.Add(userID);

        tupleType userInfo = db.getTuple("users", userID);

        string jobClassName = (string)jobClassList[(int)userInfo["job_class"]]["name"];
        string mainSkillName = (string)skillList[(int)userInfo["ultimate_skill"]]["name"];

        displayPanel.GetChild(0).gameObject.SetActive(false);
        displayInfo.gameObject.SetActive(true);

        Text nameLabel = displayInfo.Find("NameLabel").GetComponent<Text>();
        Text jobClassLabel = displayInfo.Find("JobClassLabel").GetComponent<Text>();
        Text levelLabel = displayInfo.Find("LevelLabel").GetComponent<Text>();
        Text mainSkillLabel = displayInfo.Find("MainSkillIcon").GetChild(1).GetComponent<Text>();
        Button detailButton = displayInfo.Find("DetailBtn").GetComponent<Button>();
        
        nameLabel.text = (string)userInfo["name"];
        levelLabel.text = string.Format("Lv.{0}", (int)userInfo["level"]);
        jobClassLabel.text = jobClassName;
        mainSkillLabel.text = mainSkillName;

        detailButton.onClick.AddListener(() => {
            Popup popupObj =  Popup.GetComponent<Popup>();

            string userMsg = string.Format("Lv. {0} - {1}\n메인스킬: {2}", (int)userInfo["level"], jobClassName, mainSkillName);
            userMsg += (int)room["OrderUser"] == (int)userInfo["id"] ? "\n지휘자" : "\n파티원";

            popupObj.show(
                (string)userInfo["name"],
                userMsg,
                () => {
                    room["OrderUser"] = (int)userInfo["id"];
                    popupObj.clear();
                },
                () => { popupObj.clear(); }
            );
        });

        if (userList.Count >= MaxUser) {
            CancelInvoke("partyRecruiting");
            Text recruitmentBtnLabel = RoomUI.transform.Find("BtnGroup").GetChild(1).GetChild(0).GetComponent<Text>();
            recruitmentBtnLabel.text = "모집완료";
        }
    }

    private void userInfomationPopup() {

    }

    public void partyCreateComplateBtn() {
        changeUI();
        currentUI = (int)UIStatus.Room;
        CreateUI.SetActive(false);
        RoomUI.SetActive(true);

        Text placeLabel = RoomUI.transform.Find("PlaceLabel").GetComponent<Text>();
        Text targetLabel = RoomUI.transform.Find("TargetLabel").GetComponent<Text>();

        placeLabel.text = String.Format("{0} - {1}",
            (string)placeList[(int)room["Place"] + 1]["name"],
            uiData.golaList[(int)room["Gola"]]);
        
        targetLabel.text = (string)threatList[(int)room["Threat"]]["name"];
        
        Transform mySpecUI = RoomUI.transform.Find("MySpec");
        Text nameLabel = mySpecUI.Find("NameLabel").GetComponent<Text>();
        Text jobClassLabel = mySpecUI.Find("JobClassLabel").GetComponent<Text>();
        Text levelLabel = mySpecUI.Find("LevelLabel").GetComponent<Text>();
        Text mainSkillLabel = mySpecUI.Find("MainSkillIcon").GetChild(1).GetComponent<Text>();
        
        nameLabel.text = mySpec.nameStr;
        jobClassLabel.text = (string)jobClassList[mySpec.jobClass]["name"];
        levelLabel.text = string.Format("Lv.{0}", mySpec.level);
        mainSkillLabel.text = (string)skillList[mySpec.mainSkill]["name"];
        
        Rect panelRect = UserPanel.transform.GetComponent<RectTransform>().rect;
        float groupHeight = (panelRect.height + ButtonMargin) * MaxUser;
        float top = (groupHeight / 2) - (panelRect.height / 2);

        Transform userPanelGrup = Common.createGroup(
            UserPanelGrupName,
            new Vector2(panelRect.width, groupHeight),
            new Vector2(0, -50),
            RoomUI.transform);

        Transform newUserPanel;
        for (int index = 0; index < MaxUser; index++) {
            newUserPanel = Instantiate(UserPanel).transform;
            newUserPanel.SetParent(userPanelGrup, false);
            newUserPanel.localPosition = new Vector2(0, top);
            top -= panelRect.height + ButtonMargin;
        }
    }

    public void partyBackBtnAction() {
        clearCreateUI();

        Text titleLabel = CreateUI.transform.Find("TitleLabel").GetComponent<Text>();

        Popup popupObject = Popup.GetComponent<Popup>();

        currentStatus = !Popup.activeSelf? currentStatus - 1 : currentStatus;
        popupObject.clear();
        CreateUI.transform.Find("SkillSet").gameObject.SetActive(false);

        if (currentStatus < 0) {
            currentStatus = 0;
            homeBtnAction();
        } else {
            if (currentStatus == (int)CreateStatus.Skill) {
                ArrayList setSkills = (ArrayList)room["Skill"];
                Transform group = CreateUI.transform.Find(ReviewBtnGroupName);
                Rect btnRect = SkillSelectButton.transform.GetComponent<RectTransform>().rect;
                partySkillBtnBatch("SelectBtnGroup", userSkillList.ToArray(typeof(int)) as int[], new Vector2(0, -(btnRect.height / 2)));

                CreateUI.transform.Find("SkillSet").gameObject.SetActive(true);

                Destroy(group.gameObject);
                setSkills.Clear();
            } else {
                partySelectBtnBatch(getStatusToBtnNameList(currentStatus));
            }
        }

        string key = Enum.GetName(typeof(CreateStatus), currentStatus);
        titleLabel.text = (currentStatus == (int)CreateStatus.Threat) ? uiData.golaList[(int)room[key]] : uiData.setNames[currentStatus];
    }

    public void partySkillSelectBtnAction(int selectNum) {
        ArrayList setSkills = (ArrayList)room["Skill"];

        switch (currentStatus) {
            case (int)CreateStatus.Skill:
                if (setSkills.Count < MaxSetSkill) {
                    Text skillListLabel = CreateUI.transform.Find("SkillSet").Find("SkillListLabel").GetComponent<Text>();
                    skillListLabel.text = "";

                    setSkills.Add(selectNum);

                    foreach (int skillNum in setSkills) {
                        string skillName = uiData.skillSpecNameList[skillNum];
                        skillListLabel.text += skillName + " ";
                    }
                }
                break;
            case (int)CreateStatus.Review:
                setSkills.RemoveAt(selectNum);
                partySkillBtnBatch(ReviewBtnGroupName, setSkills.ToArray(typeof(int)) as int[], new Vector2(0, 0));

                if (setSkills.Count == 0) {
                    partyBackBtnAction();
                }
                break;
        }
    }

    public void partySelectBtnAction(int selectNum) {
        Text titleLabel = CreateUI.transform.Find("TitleLabel").GetComponent<Text>();
        Popup popupObject = Popup.GetComponent<Popup>();
        
        switch (currentStatus) {
            case (int)CreateStatus.Threat:
                selectNum = selectNum + 1;

                if (!Popup.activeSelf) {
                    popupObject.show(
                        (string)threatList[selectNum]["name"],
                        (string)threatList[selectNum]["description"],
                        () => { partySelectBtnAction(0); },
                        partyBackBtnAction
                    );
                    
                    room["Threat"] = selectNum;
                } else {
                    popupObject.clear();
                    Common.clearCloneUIObj(CreateUI.transform);
                    Rect btnRect = SkillSelectButton.transform.GetComponent<RectTransform>().rect;
                    currentStatus = (int)CreateStatus.Skill;
                    CreateUI.transform.Find("SkillSet").gameObject.SetActive(true);

                    partySkillBtnBatch("SelectBtnGroup", userSkillList.ToArray(typeof(int)) as int[], new Vector2(0, -(btnRect.height/2)));
                    titleLabel.text = "스킬세팅";
                }
                break;
            case (int)CreateStatus.Skill:
                ArrayList setSkills = (ArrayList)room["Skill"];
                if (setSkills.Count >= MinSetSkill) {
                    Common.showBackPanel(CreateUI.transform, BlackPanel);
                    currentStatus = (int)CreateStatus.Review;
                    partySkillBtnBatch(ReviewBtnGroupName, setSkills.ToArray(typeof(int)) as int[], new Vector2(0, 0));

                    Transform complateBtn = CreateUI.transform.Find("CreateComplateBtn");
                    complateBtn.gameObject.SetActive(true);
                    complateBtn.SetSiblingIndex(CreateUI.transform.childCount);
                } else {
                    // warning popup
                }
                break;
            default:
                string key = Enum.GetName(typeof(CreateStatus), currentStatus);
                string[] btnNames;
                room[key] = selectNum;
                titleLabel.text = (currentStatus == (int)CreateStatus.Gola) ? uiData.golaList[selectNum] : uiData.setNames[currentStatus + 1];
                currentStatus++;
                currentStatus = currentStatus % (Enum.GetNames(typeof(CreateStatus)).Length); // overflow 방지
                btnNames = getStatusToBtnNameList(currentStatus);
                partySelectBtnBatch(btnNames);
                break;
        }
    }

    private string[] getStatusToBtnNameList(int currentNum) {
        string[] nameList = null;
        switch (currentNum) {
            case (int)CreateStatus.Gola:
                nameList = uiData.golaList;
                break;
            case (int)CreateStatus.Threat:
                nameList = tupleToNameList(threatList);
                break;
        }

        return nameList;
    }

    private string[] tupleToNameList(Dictionary<int, tupleType> tuple) {
        string[] nameList = new string[tuple.Count];

        int index = 0;
        foreach (KeyValuePair<int, tupleType> data in tuple) {
            nameList[index] = (string)data.Value["name"];
            index++;
        }

        return nameList;
    }

    private void partySkillBtnBatch(string groupName, int[] btnNames, Vector2 basePosition) {
        Transform selectBtnGroup;
        Transform groupContent;
        Rect btnRect = SkillSelectButton.transform.GetComponent<RectTransform>().rect;
        float groupWidth = ((btnRect.width + ButtonMargin) * btnNames.Length) + (ButtonMargin * 2);
        float leftStandard = -(groupWidth / 2) + (btnRect.width / 2) + ButtonMargin;

        selectBtnGroup = Common.createGroup(
            groupName,
            new Vector2(1080, 0),
            new Vector2(basePosition.x, basePosition.y),
            CreateUI.transform);

        groupContent = Common.createGroup(
            "Content",
            new Vector2(groupWidth, 0),
            new Vector2(leftStandard * -1, 0),
            selectBtnGroup);

        ScrollRect scrollObj = selectBtnGroup.gameObject.AddComponent<ScrollRect>();
        scrollObj.content = groupContent.GetComponent<RectTransform>();
        scrollObj.vertical = false;

        float left = leftStandard;

        int count = 0;
        foreach (int nameKey in btnNames) {
            Transform selectBtn = Instantiate(SkillSelectButton).transform;
            selectBtn.GetChild(0).GetComponent<Text>().text = uiData.skillSpecNameList[nameKey];
            selectBtn.SetParent(groupContent, false);
            selectBtn.localPosition = new Vector2(left, 0);

            int index = count;
            selectBtn.GetComponent<Button>().onClick.AddListener(() => { partySkillSelectBtnAction(index); });

            left += btnRect.width + ButtonMargin;
            count++;
        }
    }

    private void partySelectBtnBatch(string[] btnNames) {
        Transform selectBtnGroup;
        Rect viewRect = BottomView.transform.GetComponent<RectTransform>().rect;
        Rect btnRect = SelectButton.transform.GetComponent<RectTransform>().rect;
        int maxBtnNumber = 3; // 한라인에 배치할 버튼 갯수
        float groupWidth = (btnRect.width + ButtonMargin) * maxBtnNumber;

        selectBtnGroup = Common.createGroup(
            "SelectBtnGroup", 
            new Vector2(groupWidth, 0), 
            new Vector2(0, 0), 
            CreateUI.transform);

        float leftStandard = -(groupWidth / 2) + (btnRect.width/2);
        float newLinePoint = (groupWidth / 2) - (btnRect.width / 2); // viewRect.width - leftStandard;

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

            left += btnRect.width + ButtonMargin;
            count++;

            if (left > newLinePoint) {
                int batchLength = btnNames.Length - count;
                left = batchLength >= maxBtnNumber ? leftStandard : -(((btnRect.width + ButtonMargin) * batchLength) / 2) + (btnRect.width / 2);
                top -= btnRect.height + ButtonMargin;
            }
        }
    }

    public void partyCreateBtnAction() {
        changeUI();
        CreateUI.SetActive(true);
        currentUI = (int)UIStatus.Create;

        string[] placeNameList = tupleToNameList(placeList);

        partySelectBtnBatch(placeNameList);
        currentStatus = (int)CreateStatus.Place;
    }

    public void partyJoinBtnAction() {
        changeUI();
    }

    public void homeBtnAction() {
        hiddenUI();
        clearCreateUI();
        MainUI.SetActive(true);
        currentUI = (int)UIStatus.Main;
    }

    private void hiddenUI() {
        CreateUI.SetActive(false);
        RoomUI.SetActive(false);
        MainUI.SetActive(false);
        Popup.SetActive(false);
    } // UI를 모두 숨깁니다.

    public void battleStartBtnAction() {
        RoomState.gola = (int)room["Gola"];
        RoomState.threat = (int)room["Threat"];
        RoomState.orderUser = (int)room["OrderUser"];
        RoomState.place = (int)room["Place"] + 1;
        RoomState.skills = (ArrayList)room["Skill"];

        if (RoomState.users != null) {
            RoomState.users.Clear();
        }

        foreach(int userID in userList) {
            RoomState.addUser(userID);
        }

        SceneFaderImage.GetComponent<Fader>().fadeInStart(() => {
            SceneManager.LoadScene("battle_ground");
        });
    }

    private void clearCreateUI() {
        Common.clearCloneUIObj(CreateUI.transform);
        Transform destroyObj;
        destroyObj = CreateUI.transform.Find(ReviewBtnGroupName);
        if (destroyObj != null) {
            Destroy(destroyObj.gameObject);
        }

        CreateUI.transform.Find("SkillSet").gameObject.SetActive(false);
        CreateUI.transform.Find("CreateComplateBtn").gameObject.SetActive(false);
    } // createUI를 초기상태로 되돌립니다.

    public void prveBtnAction() {
        clearCreateUI();

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
