using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BattleSystem : MonoBehaviour {
    public GameObject HeroGroup;
    public GameObject EnemyGroup;
    public GameObject User; // 조작 하는 케릭터

    public Text ThreatLabel;

    public GameObject CharacterObject; // 복제 오브젝트

    public GameObject BottomView;

    private StatusBar mainSkillCollTimeBar;
    private StatusBar subSkillCollTimeBar;

    private Map map;

    private Score totalScore; // 누적이 되는 상황기록
    private Score localScore; // 갱신이 되는 상황기록
    private ArrayList threatScore; // 위협발생 체크용 상황기록

    private DataBase db;

    /* 맵 실시간 상황 체크 object ( 현재 맵의 상황만 기록된다. )
     {
       total : {
           kill : {
               "monsterNumber" : 0
           }
       } // 계속 누적만 된다.

       current : {
           kill : { 
               "monsterNumber" : 0 
           } // 몬스터 사냥 정보
       } // event용으로 사용되며 사용에 따라 갱신될 수 있다.
     }
    */

    IEnumerator regenCycle(mapNode regenInfo) {
        while (true) {
            regen(regenInfo.monsterList);
            yield return new WaitForSeconds(regenInfo.cycle);
        }
    } // 몬스터를 설정 시간마다 생성함

    private void monsterRegen(int monsterID) {
        DataBase db = new DataBase("DT");
        Dictionary<string, object> monsterData = db.getTuple("monster", monsterID);
        // issue - 빈 정보가 오면? 예외처리
        GameObject regenMonster = Instantiate(CharacterObject, EnemyGroup.transform);
        Character monsterInfo = regenMonster.GetComponent<Character>();
        regenMonster.transform.localPosition = new Vector2(1000, 0);
        monsterInfo.direction = -1;

        monsterInfo.infomation.movementSpeed = 1.0f;
        monsterInfo.infomation.healthPoint = (float)monsterData["hp"];

        Debug.Log(string.Format("regen [{0}]: (hp: {1}, )", (string)monsterData["name"], (float)monsterData["hp"]));

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
        foreach (mapNode regenObj in map.regen) {
            StartCoroutine(regenCycle(regenObj));
        }
    } // 몬스터를 일정시간마다 생성시키는것을 시작함

    private bool triggerCheck(mapNode trigerObj, Score threatScore) {
        bool trigger = false;
        switch (trigerObj.type) {
            case "hunt":
                int huntedCount = 0;
                foreach (int huntedMonsterNumber in trigerObj.monsterList) {
                    if (threatScore.killPoint.ContainsKey(huntedMonsterNumber)) {
                        huntedCount += threatScore.killPoint[huntedMonsterNumber];

                        ThreatLabel.text += huntedMonsterNumber + ": (" + threatScore.killPoint[huntedMonsterNumber] + "/" + trigerObj.count + ") 사냥됨. \n";
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
        foreach (threat threatObj in map.threat) {
            ThreatLabel.text += "\n[" + threatObj.title + "]\n";
            currentThreatScore = (Score)threatScore[index];

            foreach (mapNode trigerObj in threatObj.trigger) {
                currentTriger = triggerCheck(trigerObj, currentThreatScore);
                trigger = trigger && currentTriger;
            }

            if (trigger) {
                Debug.Log("["+ threatObj.title + "위협발생" + "]");

                foreach(mapNode trigerObj in threatObj.trigger) {
                    foreach (int monsterNum in trigerObj.monsterList) {
                        currentThreatScore.killPoint[monsterNum] = 0;
                    }
                    // 위협 초기화
                }

                foreach (mapNode resultObj in threatObj.result) {
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


    void Start () {
        map = Map.loadMap("json/forest");
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

        int temp = 100;
        foreach (Transform heroObj in HeroGroup.transform) {
            Character hero = heroObj.GetComponent<Character>();
            hero.infomation.energyPower += 300;
            hero.infomation.range += temp;
            temp += 100;
        }

        temp = 50;
        foreach (Transform enemyObj in EnemyGroup.transform) {
            Character enemy = enemyObj.GetComponent<Character>();
            enemy.direction = -1;
            enemy.infomation.range += temp;
            temp += 100;
        }

        Character UserObj = User.GetComponent<Character>();
        Skill testMainSkill = User.gameObject.AddComponent<Skill>();

        UserObj.mainSkillObj = testMainSkill;

        testMainSkill.targetList.Add(UserObj);
        testMainSkill.type = (int)Skill.SkillType.Holy;
        //testSkill.type = (int)Skill.SkillType.Buff;
        testMainSkill.infomation.healthPoint = 50;
        testMainSkill.infomation.energyPower = 50;
        testMainSkill.duration = 300;
        testMainSkill.coolTime = 10;

        Skill testSubSkill = User.AddComponent<Skill>();

        UserObj.subSkillObj = testSubSkill;

        testSubSkill.targetList.Add(UserObj);
        testSubSkill.type = (int)Skill.SkillType.Holy;
        testSubSkill.infomation.healthPoint = 5;
        testSubSkill.infomation.energyPower = 50;
        testSubSkill.duration = 300;
        testSubSkill.coolTime = 1;

        Transform mainSkillBtn = BottomView.transform.Find("MainSkillButton");
        Transform subvSkillBtn = BottomView.transform.Find("SubSkillButton");

        mainSkillCollTimeBar = mainSkillBtn.Find("CoolTimeBar").GetComponent<StatusBar>();
        mainSkillCollTimeBar.init(testMainSkill.coolTime, new Color(255.0f, 255.0f, 255.0f));

        subSkillCollTimeBar = subvSkillBtn.Find("CoolTimeBar").GetComponent<StatusBar>();
        subSkillCollTimeBar.init(testSubSkill.coolTime, new Color(255.0f, 255.0f, 255.0f));

        UserObj.equipments[0] = new Ability();
        UserObj.equipments[0].energyPower = 40; // 공격력 40 장비 장착 더미
    }

    public void ActiveUserMainSkill () {
        Character UserObj = User.GetComponent<Character>();
        if (UserObj.ActiveMainSkill()) {
            mainSkillCollTimeBar.runProgress();
        };
    }

    public void ActiveUserSubSkill() {
        Character UserObj = User.GetComponent<Character>();
        if (UserObj.ActiveSubSkill()) {
            subSkillCollTimeBar.runProgress();
        };
    }

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
        Character UserObj = User.GetComponent<Character>();
        UserObj.direction = setDirection;
        UserObj.status = setStatus;
        UserObj.target = null;
    }

    public void playerControlMoveStart (int setDirection) {
        playerTransform(setDirection, (int)Character.CharacterStatus.Control);
    } // 화살표 버튼을 누르기 시작합니다.

    public void playerControlMoveEnd() {
        playerTransform(1, (int)Character.CharacterStatus.Wait);
        Invoke("playerAutoTransform", 1.0f);
    } // 이동 조작을 중지합니다.

    private void playerAutoTransform() {
        playerTransform(1, (int)Character.CharacterStatus.Normal);
    } // 플레이어를 자동으로 전환합니다.

    public void playerMoveBtn (int direction) {
        Character UserObj = User.GetComponent<Character>();
        UserObj.infomation.movementSpeed = 10;
        UserObj.direction = direction;
        UserObj.move();
    }

    private void runTimePlayer() {
        Character UserObj = User.GetComponent<Character>();

        switch (UserObj.status) {
            case (int)Character.CharacterStatus.Normal:
                searchEnemy(User.transform, EnemyGroup.transform);
                UserObj.move();
                break;
            case (int)Character.CharacterStatus.Control:
                UserObj.move();
                break;
        }
    }
    
	void Update () {

        foreach (Transform heroObj in HeroGroup.transform) {
            Character hero = heroObj.GetComponent<Character>();

            switch (hero.status) {
                case (int)Character.CharacterStatus.Normal:
                    hero.move();
                    searchEnemy(heroObj, EnemyGroup.transform);
                    break;
                case (int)Character.CharacterStatus.Battle:
                    hero.move();
                    break;
                case (int)Character.CharacterStatus.Attack:
                    break;
            }
        } // heros pattern

        foreach (Transform enemyObj in EnemyGroup.transform) {
            Character enemy = enemyObj.GetComponent<Character>();

            switch (enemy.status) {
                case (int)Character.CharacterStatus.Normal:
                    enemy.move();
                    searchEnemy(enemyObj, HeroGroup.transform);
                    break;
                case (int)Character.CharacterStatus.Battle:
                    enemy.move();
                    break;
                case (int)Character.CharacterStatus.Attack:
                    break;
            }
        } // enemys pattern

        runTimePlayer();
    }
}
