using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

public class BattleSystem : MonoBehaviour {
    public GameObject HeroGroup;
    public GameObject EnemyGroup;
    public GameObject UserSpecGroup;
    public GameObject Player; // 조작 하는 케릭터

    public Text ThreatLabel;

    public GameObject CharacterObject; // 복제 오브젝트
    public GameObject UserSpecPanel; // 유저 정보 패널 복제 오브젝트(bottom view)

    public GameObject BottomView;
    public Transform InfomationGroup;
    
    private StatusBar ultimateSkillCollTimeBar;
    private StatusBar subSkillCollTimeBar1;
    private StatusBar subSkillCollTimeBar2;

    private Map map;

    private Score totalScore; // 누적이 되는 상황기록
    private Score localScore; // 갱신이 되는 상황기록
    private ArrayList threatScore; // 위협발생 체크용 상황기록

    private DataBase db;
    private RoomState.roomData roomInfo;

    private const float UIGAP = 10.0f;
    private Vector2 userStartPoint = new Vector2(-1000, 0);

    void Start() {
        // battle infomation display
        Text PlaceLabel = InfomationGroup.Find("PlaceLabel").GetComponent<Text>();
        Text TargetLabel = InfomationGroup.Find("TargetLabel").GetComponent<Text>();
        Text ThreatLabel = InfomationGroup.Find("ThreatLabel").GetComponent<Text>();

        db = new DataBase("DT");
        roomInfo = RoomState.loadRoomData("json/quest_board");

        RoomState.place = 1;
        RoomState.threat = 1;
        
        RoomState.addUser(2);
        RoomState.addUser(3);
        RoomState.addUser(4);
        RoomState.addUser(5);
        
        tupleType place = db.getTuple("places", RoomState.place);
        tupleType threat = db.getTuple("threats", RoomState.threat);

        PlaceLabel.text = string.Format("{0} - {1}", place["name"] as string, roomInfo.golaList[RoomState.gola]);
        TargetLabel.text = threat["name"] as string;

        Debug.Log(place["name"] as string + ", " + roomInfo.golaList[RoomState.gola] + ", " + threat["name"] as string + ", ");

        map = Map.loadMap(string.Format("json/threat/{0}", threat["file"] as string));
        runRegen(); // 설정한 대로 몬스터들을 재 매 설정 시간마다 몬스터를 생성시킵니다.

        if (map == null) {
            Debug.Log("map load error");
        }

        totalScore = new Score();
        localScore = new Score();
        threatScore = new ArrayList();

        for (int index = 0; index < map.threat.Count; index++) {
            threatScore.Add(new Score());
        }

        usersInit();
        playerInit();
    }

    private void gameOver() {
        Debug.Log("~ game over ~");
    }

    IEnumerator regenCycle(Map.mapNode regenInfo) {
        while (true) {
            regen(regenInfo.monsterList);
            yield return new WaitForSeconds(regenInfo.cycle);
        }
    } // 몬스터를 설정 시간마다 생성함

    private void monsterRegen(int monsterID) {
        tupleType monsterData = db.getTuple("monsters", monsterID);
        // issue - 빈 정보가 오면? 예외처리
        GameObject regenMonster = Instantiate(CharacterObject, EnemyGroup.transform);
        Character monsterInfo = regenMonster.GetComponent<Character>();
        Transform monsterVisual = regenMonster.transform.Find("Sprite");
        Image monsterSprite = monsterVisual.GetComponent<Image>();

        monsterSprite.sprite = Resources.Load<Sprite>((string)monsterData["sprite"]);
        monsterVisual.localScale = new Vector3(-1, 1, 1);

        regenMonster.transform.localPosition = new Vector2(1000, 0);

        monsterInfo.setting(monsterData, -1);

        Debug.Log(string.Format("regen [{0}]: (hp: {1}, )", (string)monsterData["name"], (float)monsterData["health_point"]));

        StatusBar headHpBar = regenMonster.transform.Find("HpBar").GetComponent<StatusBar>();
        headHpBar.init((float)monsterData["health_point"], new Color(255.0f, 0, 0));
        monsterInfo.hpBar.Add(headHpBar);

        StatusBar headDelayBar = regenMonster.transform.Find("DelayBar").GetComponent<StatusBar>();
        headDelayBar.init(0, new Color(0, 0, 255.0f));
        monsterInfo.delayBar.Add(headDelayBar);

        monsterInfo.destroyCallback = (() => {
            if (localScore.killPoint.ContainsKey(monsterID)) {
                totalScore.killPoint[monsterID] += 1;
                localScore.killPoint[monsterID] += 1;
                foreach (Score score in threatScore) {
                    score.killPoint[monsterID] += 1;
                }
            }
            else {
                totalScore.killPoint.Add(monsterID, 1);
                localScore.killPoint.Add(monsterID, 1);
                foreach (Score score in threatScore ) {
                    score.killPoint.Add(monsterID, 1);
                }
            }

            Debug.Log("사망[" + monsterID + "]:" + localScore.killPoint[monsterID]);

            ThreatLabel.text = "";

            threat();
        });
    } // 몬스터 생성

    private void regen(List<int> monsterList) {
        foreach (int monsterNumber in monsterList) {
            monsterRegen(monsterNumber);
            // issue - 몬스터 db조회하여 이미지나 스탯설정할것
        }
    } // 몬스터 생성

    private void runRegen() {
        foreach (Map.mapNode regenObj in map.regen) {
            StartCoroutine(regenCycle(regenObj));
        }
    } // 몬스터를 일정시간마다 생성시키는것을 시작함

    private bool triggerCheck(Map.mapNode trigerObj, Score threatScore) {
        bool trigger = false;
        switch (trigerObj.type) {
            case "hunt":
                int huntedCount = 0;
                foreach (int huntedMonsterNumber in trigerObj.monsterList) {
                    if (threatScore.killPoint.ContainsKey(huntedMonsterNumber)) {
                        huntedCount += threatScore.killPoint[huntedMonsterNumber];

                        ThreatLabel.text += huntedMonsterNumber + ": (" + threatScore.killPoint[huntedMonsterNumber] + "/" + trigerObj.count + ") 사냥됨.";
                    }
                    else {
                        return false;
                    }
                }
                
                if (huntedCount >= trigerObj.count) {
                    trigger = true;
                }
                break;
        }

        return trigger;
    } // 위협 조건하나가 성립됐는지 확인

    private void threat() {
        bool trigger = true;
        bool currentTriger = true;
        Score currentThreatScore;
        int index = 0;
        foreach (Map.threatNode threatObj in map.threat) {
            ThreatLabel.text += "\n[" + threatObj.title + "]\n";
            currentThreatScore = (Score)threatScore[index];

            foreach (Map.mapNode trigerObj in threatObj.trigger) {
                currentTriger = triggerCheck(trigerObj, currentThreatScore);
                trigger = trigger && currentTriger;
            }

            if (trigger) {
                Debug.Log("["+ threatObj.title + "위협발생" + "]");

                foreach(Map.mapNode trigerObj in threatObj.trigger) {
                    foreach (int monsterNum in trigerObj.monsterList) {
                        currentThreatScore.killPoint[monsterNum] = 0;
                    }
                    // 위협 초기화
                }

                foreach (Map.mapNode resultObj in threatObj.result) {
                    switch (resultObj.type) {
                        case "produce":
                            foreach (int monster in resultObj.monsterList) {
                                // 몬스터 생성
                                for (int monsterCount=0; monsterCount < resultObj.produce; monsterCount++) {
                                    monsterRegen(monster);
                                }
                            }
                            break;
                    }
                }
            }

            index++;
            trigger = true;
        } // 위협들 체크
    }

    private void playerInit() {
        Character playerObj = Player.GetComponent<Character>();
        Skill testMainSkill = Player.gameObject.AddComponent<Skill>();

        playerObj.ultimateSkillObj = testMainSkill;

        testMainSkill.targetList.Add(playerObj);
        testMainSkill.type = (int)Skill.SkillType.Holy;
        //testSkill.type = (int)Skill.SkillType.Buff;
        testMainSkill.infomation.healthPoint = 50;
        testMainSkill.infomation.energyPower = 50;
        testMainSkill.duration = 300;
        testMainSkill.coolTime = 10;

        Skill testSubSkill = Player.AddComponent<Skill>();

        playerObj.subSkillObj1 = testSubSkill;

        testSubSkill.targetList.Add(playerObj);
        testSubSkill.type = (int)Skill.SkillType.Holy;
        testSubSkill.infomation.healthPoint = 5;
        testSubSkill.infomation.energyPower = 50;
        testSubSkill.duration = 300;
        testSubSkill.coolTime = 1;

        Transform ultimateSkillBtn = BottomView.transform.Find("UltimateSkillButton");
        Transform subvSkillBtn1 = BottomView.transform.Find("SubSkillButton1");
        Transform subvSkillBtn2 = BottomView.transform.Find("SubSkillButton2");

        ultimateSkillCollTimeBar = ultimateSkillBtn.Find("CoolTimeBar").GetComponent<StatusBar>();
        ultimateSkillCollTimeBar.init(testMainSkill.coolTime, new Color(255.0f, 255.0f, 255.0f));

        subSkillCollTimeBar1 = subvSkillBtn1.Find("CoolTimeBar").GetComponent<StatusBar>();
        subSkillCollTimeBar1.init(testSubSkill.coolTime, new Color(255.0f, 255.0f, 255.0f));

        subSkillCollTimeBar2 = subvSkillBtn2.Find("CoolTimeBar").GetComponent<StatusBar>();
        subSkillCollTimeBar2.init(testSubSkill.coolTime, new Color(255.0f, 255.0f, 255.0f));

        playerObj.equipments[0] = new Ability();
        playerObj.equipments[0].energyPower = 40; // 공격력 40 장비 장착 더미

        GameObject userSpecPanel = UserSpecGroup.transform.GetChild(0).gameObject;
        tupleType userInfo = db.getTuple("users", 1);
        float originPositionY = Player.transform.localPosition.y;

        panelSetting(userSpecPanel, userInfo, Player);
        userObjectSetting(Player, userInfo);

        Player.transform.localPosition = new Vector2(userStartPoint.x, originPositionY);
    } // 플레이어 셋팅

    private void userPanelSetting(GameObject setUserPanel, tupleType userInfo) {
        Transform specInfo = setUserPanel.transform.Find("SpecInfo");
        RectTransform panelRect = setUserPanel.GetComponent<RectTransform>();
        int thisIndex = UserSpecGroup.transform.childCount - 1;
        GameObject thisUserObject = HeroGroup.transform.GetChild(HeroGroup.transform.childCount - 1).gameObject;
        float baseY = UserSpecGroup.transform.GetChild((thisIndex - 1)).localPosition.y;

        panelSetting(setUserPanel, userInfo, thisUserObject);

        panelRect.localPosition = new Vector2(0, baseY - UIGAP - panelRect.rect.height);
    }

    private void panelSetting(GameObject setUserPanel, tupleType userInfo, GameObject userObject) {
        Transform specInfo = setUserPanel.transform.Find("SpecInfo");
        Button targetingBtn = specInfo.Find("TargetingBtn").GetComponent<Button>();
        Text nameLabel = specInfo.Find("NameLabel").GetComponent<Text>();
        Text levelLabel = specInfo.Find("LevelLabel").GetComponent<Text>();

        nameLabel.text = (string)userInfo["name"];
        levelLabel.text = string.Format("Lv.{0}", ((int)userInfo["level"]).ToString());

        targetingBtn.onClick.AddListener(() => { userTargetingBtn(userObject); });

        Character setUser = userObject.GetComponent<Character>();
        StatusBar uiHpBar = specInfo.Find("HpBar").GetComponent<StatusBar>();
        uiHpBar.init((float)userInfo["health_point"], new Color(255.0f, 0, 0));
        setUser.hpBar.Add(uiHpBar);
    }

    private void userObjectSetting(GameObject setUser, tupleType userInfo) {
        Character user;
        RectTransform userRect;
        Image userVisual;
        float positionX;
        int temp = 100;

        user = setUser.GetComponent<Character>();
        user.setting(userInfo, 1);

        userRect = setUser.GetComponent<RectTransform>();
        positionX = userStartPoint.x + (userRect.rect.width * (HeroGroup.transform.childCount + 1) + UIGAP);
        // Player Object 포함한 계산

        userRect.localPosition = new Vector2(positionX, 0);

        userVisual = setUser.transform.Find("Sprite").GetComponent<Image>();
        userVisual.sprite = Resources.Load<Sprite>((string)userInfo["sprite"]);

        // dummy
        user.infomation.energyPower += 300;
        user.infomation.range += temp;
        user.infomation.holyPower = 10;

        user.direction = +1;
        temp += 100;
        positionX += userRect.rect.width;

        StatusBar headHpBar = setUser.transform.Find("HpBar").GetComponent<StatusBar>();
        headHpBar.init((float)userInfo["health_point"], new Color(255.0f, 0, 0));
        user.hpBar.Add(headHpBar);

        StatusBar headDelayBar = setUser.transform.Find("DelayBar").GetComponent<StatusBar>();
        headDelayBar.init(0, new Color(0, 0, 255.0f));
        user.delayBar.Add(headDelayBar);

        int thisPanelIndex = HeroGroup.transform.childCount - 1;
        user.destroyCallback = (() => {
            UserSpecGroup.transform.GetChild(thisPanelIndex).Find("DeathStatus").gameObject.SetActive(true);

            if (HeroGroup.transform.childCount >= 0) {
                this.gameOver();
            }
        });
    }

    private void usersInit() {
        GameObject entryUser;
        GameObject userSpecPanel;
        tupleType userInfo;

        if (RoomState.users != null) {
            for (int index = 0; index < RoomState.users.Count; index++) {
                userInfo = db.getTuple("users", (int)RoomState.users[index]);
                entryUser = Instantiate(CharacterObject, HeroGroup.transform);
                userSpecPanel = Instantiate(UserSpecPanel, UserSpecGroup.transform);

                userObjectSetting(entryUser, userInfo);
                userPanelSetting(userSpecPanel, userInfo);
            }
        } // 파티에 유저가 있는지 체크
    } // 유저들 셋팅

    public void userTargetingBtn(GameObject targetObject) {
        Character UserObj = Player.GetComponent<Character>();

        UserObj.target = targetObject;
    } // 유저를 타겟으로 설정합니다.

    public void activeUltimateSkill() {
        Character UserObj = Player.GetComponent<Character>();
        if (UserObj.activeUltimateSkill()) {
            ultimateSkillCollTimeBar.runProgress();
        };
    }

    public void activeUserSubSkill1() {
        Character UserObj = Player.GetComponent<Character>();
        if (UserObj.activeSubSkill1()) {
            subSkillCollTimeBar1.runProgress();
        };
    }

    public void activeUserSubSkill2() {
        Character UserObj = Player.GetComponent<Character>();
        if (UserObj.activeSubSkill2()) {
            subSkillCollTimeBar2.runProgress();
        };
    }

    private void searchAlliance(Transform targetObj, Transform allianceGroup) {
        Character target = targetObj.GetComponent<Character>();
        float searchLowHP = -1;
        Character allianceInfo;

        foreach (Transform allianceObj in allianceGroup) {
            allianceInfo = allianceObj.GetComponent<Character>();

            if (searchLowHP == -1 || searchLowHP > allianceInfo.currentHealthPoint) {
                searchLowHP = allianceInfo.currentHealthPoint;
                target.target = allianceObj.gameObject;
            }
        }

        target.status = (int)Character.CharacterStatus.Battle;
    } // 체력이 가장 낮은 아군을 찾습니다.

    private void searchEnemy(Transform targetObj, Transform enemyGroup) {
        Character target = targetObj.GetComponent<Character>();
        Vector2 targetPosition = targetObj.position;

        float minDistance = -1;
        foreach (Transform enemyObj in enemyGroup) {
            target.aggroCheck(enemyObj);

            Vector2 enemyPosition = enemyObj.position;

            float distance = Vector2.Distance(enemyPosition, targetPosition);
            if (target.aggroCheck(enemyObj) && (minDistance == -1 || minDistance > distance)) {
                minDistance = distance;
                target.target = enemyObj.gameObject;
            }

            if (minDistance != -1) {
                target.status = (int)Character.CharacterStatus.Battle;
            }
        }
    } // 적을 찾습니다.

    private void playerTransform(int setDirection, int setStatus) {
        Character UserObj = Player.GetComponent<Character>();
        UserObj.direction = setDirection;
        UserObj.status = setStatus;
        UserObj.target = null;
        UserObj.cancelCurrentBeforeDelay();
    } // 플레이어의 이동방향과 상태를 바꿉니다.

    public void playerControlMoveStart (int setDirection) {
        CancelInvoke("playerAutoTransform");
        playerTransform(setDirection, (int)Character.CharacterStatus.Control);
    } // 화살표 버튼을 누르기 시작합니다.

    public void playerControlMoveEnd() {
        playerTransform(1, (int)Character.CharacterStatus.Wait);
        Invoke("playerAutoTransform", 1.0f);
    } // 이동 조작을 중지합니다.

    private void playerAutoTransform() {
        playerTransform(1, (int)Character.CharacterStatus.Normal);
    } // 플레이어를 자동으로 전환합니다.

    private void basePattern(Transform targetObj, Transform colleagueGroup, Transform enemyGroup) {
        Character target = targetObj.GetComponent<Character>();

        switch (target.status) {
            case (int)Character.CharacterStatus.Normal:
                target.move();
                if (target.attackType == (int)Character.CharacterAttackType.Heal) {
                    searchAlliance(targetObj.transform, colleagueGroup.transform);
                } else {
                    searchEnemy(targetObj.transform, enemyGroup.transform);
                }
                break;
            case (int)Character.CharacterStatus.Battle:
                target.battle();
                break;
            case (int)Character.CharacterStatus.Attack:
                break;
            case (int)Character.CharacterStatus.Control:
                target.move();
                break;
        }
    }
    
	void Update () {
        foreach (Transform heroObj in HeroGroup.transform) {
            basePattern(heroObj.transform, HeroGroup.transform, EnemyGroup.transform);
        } // heros pattern

        foreach (Transform enemyObj in EnemyGroup.transform) {
            basePattern(enemyObj.transform, EnemyGroup.transform, HeroGroup.transform);
        } // enemys pattern
    }
}
