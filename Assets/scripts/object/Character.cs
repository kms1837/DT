﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using tupleType = System.Collections.Generic.Dictionary<string, object>;

public class Character : MonoBehaviour
{
    public Ability infomation; // 케릭터 정보
    public int status; // 케릭터 상태
    private int prevStatus; // 전 상태
    public int type; // 케릭터 타입
    public int attackType;

    public float currentHealthPoint; // HP
    public float currentManaPoint; // MP

    public GameObject target; // 공격대상
    public int direction; // 방향
    private int originDirection; // 평소 이동 방향

    public enum CharacterStatus { Normal, Battle, Attack, Control, Wait }
    // 평상시, 전투모드, 공격중, 조작중, 대기중

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

    void Awake () {
        Vector2 objPosition = this.transform.position;
        Rect objRect = this.gameObject.GetComponent<RectTransform>().rect;

        infomation = new Ability();

        status = (int)CharacterStatus.Normal;

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

        }
    }
	
	void Update () {
        updateUI();
    }

    public void setting(tupleType settingData, int inDirection) {
        this.attackType = (int)settingData["attack_type"];

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
            float damage = infomation.energyPower;

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

        delay(infomation.afterDelay, AfterDelayColor, "backStatus");
    }

    private void backStatus() {
        status = prevStatus;
    } // 이전상태로 돌아감

    private void delay(float time, Color delayColor, string callBack) {
        foreach (StatusBar bar in delayBar) {
            bar.setMaximum(time);
            bar.setColor(delayColor);
            bar.runProgress();
        }
        
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
        if (nullCheck && (status != (int)CharacterStatus.Control)) {
            this.status = (int)CharacterStatus.Normal;
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
            this.prevStatus = status;
            this.status = (int)CharacterStatus.Attack;
            beforeDelayActionStr = "attack";

            foreach (StatusBar bar in delayBar) {
                bar.setMaximum(infomation.beforeDelay);
            }

            CancelInvoke(beforeDelayActionStr);
            delay(infomation.beforeDelay, BeforeDelayColor, beforeDelayActionStr);
        } else {
            float temp = this.gameObject.transform.position.x - target.transform.position.x;
            direction = temp >= 0 ? -1 : +1;

            this.move();
        }

        if (this.infomation.aggroRadius < distance) {
            target = null;
            status = (int)CharacterStatus.Normal;
        } // 어그로 해제
        
    } // 행동

    public bool activeUltimateSkill() {
        return ultimateSkillObj.activation();
    } // 메인 스킬 발동

    public bool activeSubSkill1() {
        return subSkillObj1.activation();
    } // 서브 스킬1 발동

    public bool activeSubSkill2() {
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
