using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

public class Character : MonoBehaviour
{
    public Ability infomation; // 케릭터 정보
    public int action; // 케릭터 행동(이동, 공격 등)
    private int nextAction; // 다음상태 예약
    private int prevAction; // 전 상태
    public int status; // 케릭터 상태(화상, 빙결 등)
    public int type; // 케릭터 타입
    public int attackType;

    public float currentHealthPoint; // HP
    public float currentManaPoint; // MP

    public GameObject target; // 공격대상
    public int direction; // 방향
    private int originDirection; // 평소 이동 방향

    public enum CharacterAction { Normal, Battle, Attack, Control, Wait, BeforeDelay, AfterDelay, Constraint, Ultimate, Skill1, Skill2 }
    // 평상시, 전투모드, 공격중, 조작중, 대기중, 선딜레이중, 후딜레이중, 행동제약, 궁극기사용중, 스킬1 사용중, 스킬2 사용중

    public enum CharacterType { Hero, NPC, Monster, Boss }
    public enum CharacterAttackType { Attack, Heal }

    private string beforeDelayActionStr; // 선딜레이 중인 함수 이름(invoke 중인 상태)

    public Ability[] equipments; // 장비

    public ArrayList buffList; // 버프, 디버프 리스트

    // Skill Objects
    public Skill ultimateSkillObj;
    public Skill subSkillObj1;
    public Skill subSkillObj2;

    // ui
    public ArrayList hpBar; // hpbar ui 목록
    public ArrayList delayBar; // hpbar ui 목록

    public UnityEngine.Events.UnityAction destroyCallback; // 케릭터 사망시 콜백 설정

    Color AfterDelayColor = new Color(255.0f, 150.0f, 0.0f);
    Color BeforeDelayColor= new Color(0.0f, 0.0f, 255.0f);

    private const float baseWidth = 150;

    void Awake () {
        Vector2 objPosition = this.transform.position;
        Rect objRect = this.gameObject.GetComponent<RectTransform>().rect;

        infomation = new Ability();

        this.action = (int)CharacterAction.Normal;

        target = null;
        
        // base setting
        infomation.movementSpeed = 2;
        direction = 1;
        infomation.beforeDelay = 2.0f;
        infomation.afterDelay = 2.0f;
        infomation.energyPower = 5;
        infomation.healthPoint = 100;
        currentHealthPoint = 100;

        infomation.aggroRadius = objRect.width + 200;
        infomation.range = objRect.width + 10;
        

        beforeDelayActionStr = string.Empty;

        hpBar = new ArrayList();
        delayBar = new ArrayList();

        buffList = new ArrayList();
        equipments = new Ability[5];

        for (int i=0; i<5; i++) {
            equipments[i] = new Ability();
        }
    }

    private void updateUI() {
        Vector2 objPosition = this.transform.position;

        try {
            foreach(StatusBar bar in hpBar) {
                bar.setCurrent(currentHealthPoint);
            }
        } catch (NullReferenceException err) {
            Debug.Log(err);
        }
    }
	
	void Update () {
        updateUI();
    }

    public void setSprite(string filePath) {
        Transform spriteObject = this.transform.Find("Sprite");
        Image setSprite = spriteObject.GetComponent<Image>();
        Sprite loadSprite = Resources.Load<Sprite>(filePath);
        RectTransform setSize = this.transform.Find("Sprite").GetComponent<RectTransform>();

        float setHeight = loadSprite.rect.height * (baseWidth / loadSprite.rect.width);

        setSprite.sprite = loadSprite;
        setSize.sizeDelta = new Vector2(baseWidth, setHeight);
    }

    public void setSprite(string filePath, Vector2 size) {
        setSprite(filePath);

        RectTransform setSize = this.transform.Find("Sprite").GetComponent<RectTransform>();
        setSize.sizeDelta = size;
    }

    public void setting(tupleType settingData, int inDirection) {
        this.attackType = (int)settingData["attack_type"];

        this.infomation.nameStr = (string)settingData["name"];

        this.infomation.healthPoint = (float)settingData["health_point"];
        this.currentHealthPoint = (float)settingData["health_point"];
        
        this.infomation.beforeDelay = (float)settingData["before_delay"];
        this.infomation.afterDelay = (float)settingData["after_delay"];

        this.infomation.power = (float)settingData["power"];

        this.infomation.movementSpeed = (float)settingData["movement_speed"];

        direction = inDirection;
        originDirection = inDirection;
    }

    public void attack () {
        if (targetCheck()) {
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;
        float distance = Vector2.Distance(target.transform.position, currentPosition);

        if (infomation.range >= distance) {
            Character attackTarget = target.GetComponent<Character>();
            float damage = infomation.power;

            if (attackType == (int)CharacterAttackType.Heal) {
                attackTarget.heal(this.infomation.power);
                if (attackTarget.currentHealthPoint >= attackTarget.infomation.healthPoint) {
                    target = null;
                } // 체력이 가득차서 타겟을 변경함

            } else {
                foreach (Skill buff in buffList) {
                    damage += buff.infomation.energyPower;
                }

                foreach (Ability equipment in equipments) {
                    damage += equipment.energyPower;
                }

                attackTarget.hit(damage);
            }
        } // 공격 성공

        runAfterDelay();
    }

    public void setNextAction(int inNextAction) {
        this.nextAction = inNextAction;
    }

    private void runNextAction() {
        backAction();

        switch (this.nextAction) {
            case (int)CharacterAction.Ultimate:
                this.activeUltimateSkill();
                break;
            case (int)CharacterAction.Skill1:
                this.activeSubSkill1();
                break;
            case (int)CharacterAction.Skill2:
                this.activeSubSkill2();
                break;
        }

        this.nextAction = prevAction;
    } // 다음 행동을 행함

    private void backAction() {
        this.action = prevAction;
    }

    private void runBeforeDelay() {
        float finalDelay = this.infomation.beforeDelay;
        foreach (Skill buff in buffList) {
            finalDelay += buff.infomation.beforeDelay;
        } // 버프, 디버프로 인한 딜레이 추가
        delay(finalDelay, (int)Character.CharacterAction.BeforeDelay, BeforeDelayColor, beforeDelayActionStr);
    }

    private void runAfterDelay() {
        float finalDelay = this.infomation.afterDelay;
        foreach (Skill buff in buffList) {
            finalDelay += buff.infomation.afterDelay;
        } // 버프, 디버프로 인한 딜레이 추가
        delay(infomation.afterDelay, (int)Character.CharacterAction.AfterDelay, AfterDelayColor, "runNextAction");
    }

    private void delay(float time, int setStatus, Color delayColor, string callBack) {
        foreach (StatusBar bar in delayBar) {
            bar.setMaximum(time);
            bar.setColor(delayColor);
            bar.runProgress();
        }

        this.action = setStatus;

        Invoke(callBack, time);
    }

    public void cancelCurrentBeforeDelay() {
        if (beforeDelayActionStr != string.Empty) {
            foreach (StatusBar bar in delayBar) {
                bar.stopProgress();
            }

            CancelInvoke(beforeDelayActionStr);
            beforeDelayActionStr = string.Empty;
        }
    }

    public void heal(float healPower) {
        this.transform.Find("Sprite").GetComponent<Image>().color = new Color(0, 255, 0);
        Invoke("clear", 0.1f);

        this.currentHealthPoint = this.currentHealthPoint >= this.infomation.healthPoint ? this.infomation.healthPoint : this.currentHealthPoint + healPower;
    } // 회복받음

    public void hit(float damage) {
        this.transform.Find("Sprite").GetComponent<Image>().color = new Color(255, 0, 0);
        Invoke("clear", 0.1f);

        cancelCurrentBeforeDelay();

        currentHealthPoint -= damage;

        this.action = (int)Character.CharacterAction.Battle;

        if (currentHealthPoint <= 0) {
            dead();            
        }
    } // 공격받음

    private void dead() {
        if (destroyCallback != null) {
            destroyCallback();
        }

        Destroy(this.gameObject);
    }

    private void clear() {
        this.transform.Find("Sprite").GetComponent<Image>().color = new Color(255, 255, 255);
    }

    private bool targetCheck() {
        bool nullCheck = (target == null);
        if (nullCheck && (action != (int)CharacterAction.Control)) {
            this.action = (int)CharacterAction.Normal;
            direction = originDirection;
        }
        
        // 타켓이 없을때 유저 직접 컨트롤 상태이면 유지하고 컨트롤 상태가 아니면 일반 상태로 돌린다.

        return nullCheck;
    } // 타겟이 있는지 없는지 확인 없으면 일반상태로 바꾼다.

    public void move() {
        Vector2 currentPosition = this.gameObject.transform.position;
        this.gameObject.transform.position = new Vector2(currentPosition.x + (direction * infomation.movementSpeed), currentPosition.y);
    } // 이동

    public void battle() {
        if (targetCheck()) {
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;

        float distance = Vector2.Distance(target.transform.position, currentPosition);

        if (this.infomation.range >= distance) {
            this.prevAction = action;
            this.action = (int)CharacterAction.Attack;
            beforeDelayActionStr = "attack";

            foreach (StatusBar bar in delayBar) {
                bar.setMaximum(infomation.beforeDelay);
            }

            CancelInvoke(beforeDelayActionStr);
            runBeforeDelay();
        } else {
            float temp = this.gameObject.transform.position.x - target.transform.position.x;
            direction = temp >= 0 ? -1 : +1;

            this.move();
        }

        if (this.infomation.aggroRadius < distance) {
            target = null;
            action = (int)CharacterAction.Normal;
        } // 어그로 해제
    } // 행동

    private bool actionCheck() {
        bool result = ( this.action == (int)Character.CharacterAction.AfterDelay    ||
                        this.action == (int)Character.CharacterAction.BeforeDelay   ||
                        this.action == (int)Character.CharacterAction.Constraint);

        if (result) {
            Debug.Log("행동불가");
        }

        return result;
    } // 행동이 가능한지 체크하고 행동이 불가능하면 이전 행동으로 되돌림

    public bool activeUltimateSkill() {
        if (actionCheck()) {
            return false;
        }

        return ultimateSkillObj.activation();
    } // 메인 스킬 발동

    public bool activeSubSkill1() {
        if (actionCheck()) {
            return false;
        }

        return subSkillObj1.activation();
    } // 서브 스킬1 발동

    public bool activeSubSkill2() {
        if (actionCheck()) {
            return false;
        }

        return subSkillObj2.activation();
    } // 서브 스킬2 발동

    public bool aggroCheck(Transform target) {
        Vector2 targetPosition = target.position;
        bool returnData = false;

        float distance = Vector2.Distance(targetPosition, this.gameObject.transform.position);
        if (this.infomation.aggroRadius >= distance) {
            returnData = true;
        }

        return returnData;
    } // 대상탐색 - 대상이 어그로 범위 안에 있는지 체크합니다.
}
