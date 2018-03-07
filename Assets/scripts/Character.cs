using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : Ability
{
    public int status; // 케릭터 상태
    private int prevStatus; // 전 상태
    public int type; // 케릭터 타입

    public float currentHealthPoint; // HP
    public float currentManaPoint; // MP

    public GameObject target; // 공격대상

    public int direction; // 방향

    public enum CharacterStatus { Normal, Battle, Attack }
    // 평상시, 전투모드, 공격중

    public enum CharacterType { Hero, NPC, Monster, Boss }

    private string beforeDelayActionStr; // 선딜레이 중인 함수 이름(invoke 중인 상태)

    Ability[] equipment = new Ability[5]; // 장비

    public ArrayList buffList; // 버프, 디버프 리스트

    // skill Number
    public int passive;
    public int mainSkill;
    public int sideSkill;

    // Skill Object
    public Skill mainSkillObj;
    public Skill subSkillObj;

    // ui
    private StatusBar hpBar;
    private StatusBar delayBar;

    void Start () {
        Vector2 objPosition = this.transform.position;
        Rect objRect = this.gameObject.GetComponent<RectTransform>().rect;

        status = (int)CharacterStatus.Normal;

        target = null;

        movementSpeed = 2;
        direction = 1;
        beforeDelay = 2.0f;
        afterDelay = 2.0f;
        energyPower = 5;
        healthPoint = 100;
        currentHealthPoint = 100;

        aggroRadius = objRect.width + 200;
        range = objRect.width + 10;

        beforeDelayActionStr = string.Empty;

        hpBar = this.gameObject.transform.Find("HpBar").GetComponent<StatusBar>();
        hpBar.init(healthPoint, new Color(255.0f, 0, 0));

        delayBar = this.gameObject.transform.Find("DelayBar").GetComponent<StatusBar>();
        delayBar.init(0, new Color(0, 0, 255.0f));

        buffList = new ArrayList();
    }

    private void updateUI() {
        Vector2 objPosition = this.transform.position;
        //hpBar.setPosition(new Vector2(objPosition.x, hpBar.position.y));
        //delayBar.setPosition(new Vector2(objPosition.x, delayBar.position.y));

        try { 
            hpBar.setCurrent(currentHealthPoint);
        } catch (NullReferenceException err) {

        }
    }
	
	void Update () {
        updateUI();
    }

    public void setTarget() {

    }

    public void attch () {
        if (target == null) {
            status = (int)CharacterStatus.Normal;
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;
        float distance = Vector2.Distance(target.transform.position, currentPosition);

        if (range >= distance) {
            Character attchTarget = target.GetComponent<Character>();
            float damage = this.energyPower;
            foreach(Skill buff in buffList) {
                damage += buff.energyPower;
            }

            attchTarget.hit(damage);
            delay(afterDelay, "backStatus");
        }
    }

    private void backStatus() {
        status = prevStatus;
    } // 이전상태로 돌아감

    private void delay(float time, string callBack) {
        delayBar.runProgress(time);
        Invoke(callBack, time);
    }

    public void hit(float damage) {
        this.gameObject.GetComponent<Image>().color = new Color(255, 0, 0);
        Invoke("clear", 0.1f);

        if (beforeDelayActionStr != string.Empty) {
            CancelInvoke(beforeDelayActionStr);
            beforeDelayActionStr = string.Empty;
            status = (int)CharacterStatus.Battle;
        }

        currentHealthPoint -= damage;

        if (currentHealthPoint <= 0) {
            // dead
            Destroy(this.gameObject);
        }
    } // 공격받음

    private void clear() {
        this.gameObject.GetComponent<Image>().color = new Color(255, 255, 255);
    }

    public void move() {
        Vector2 currentPosition = this.gameObject.transform.position;

        status = target == null ? (int)CharacterStatus.Normal : status;

        if (status == (int)CharacterStatus.Battle && target != null) {
            float distance = Vector2.Distance(target.transform.position, currentPosition);

            if (this.range >= distance) {
                this.prevStatus = status;
                this.status = (int)CharacterStatus.Attack;
                beforeDelayActionStr = "attch";

                delayBar.setMaximum(beforeDelay);
                delay(beforeDelay, beforeDelayActionStr);
            }

            if (this.aggroRadius < distance) {
                target = null;
                status = (int)CharacterStatus.Normal;
            } // 어그로 해제
        }

        this.gameObject.transform.position = new Vector2(currentPosition.x + (direction * movementSpeed), currentPosition.y);
        
    } // 이동

    public bool ActiveMainSkill() {
        return mainSkillObj.activation();
    } // 메인 스킬 발동

    public bool ActiveSubSkill() {
        return subSkillObj.activation();
    } // 서브 스킬 발동

    public bool aggroCheck(Transform target) {
        Vector2 targetPosition = target.position;
        bool returnData = false;

        float distance = Vector2.Distance(targetPosition, this.gameObject.transform.position);
        if (this.aggroRadius >= distance) {
            returnData = true;
        }

        return returnData;
    } // 대상탐색 - 대상이 어그로 범위 안에 있는지 체크합니다.
}
