using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public Ability infomation; // 케릭터 정보
    public int status; // 케릭터 상태
    private int prevStatus; // 전 상태
    public int type; // 케릭터 타입

    public float currentHealthPoint; // HP
    public float currentManaPoint; // MP

    public GameObject target; // 공격대상
    public int direction; // 방향

    public enum CharacterStatus { Normal, Battle, Attack, Control, Wait }
    // 평상시, 전투모드, 공격중, 조작중, 대기중

    public enum CharacterType { Hero, NPC, Monster, Boss }

    private string beforeDelayActionStr; // 선딜레이 중인 함수 이름(invoke 중인 상태)

    public Ability[] equipments; // 장비

    public ArrayList buffList; // 버프, 디버프 리스트

    // Skill Object
    public Skill mainSkillObj;
    public Skill subSkillObj;

    // ui
    public ArrayList hpBar; // hpbar ui 목록
    public ArrayList delayBar; // hpbar ui 목록

    public UnityEngine.Events.UnityAction destroyCallback; // 케릭터 사망시 콜백 설정

    void OnDestroy () {
        if (destroyCallback != null) {
            destroyCallback();
        }
    }

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

    public void setTarget() {

    }

    public void attch () {
        if (target == null) {
            status = (int)CharacterStatus.Normal;
            return;
        }

        Vector2 currentPosition = this.gameObject.transform.position;
        float distance = Vector2.Distance(target.transform.position, currentPosition);

        if (infomation.range >= distance) {
            Character attchTarget = target.GetComponent<Character>();
            float damage = infomation.energyPower;

            foreach(Skill buff in buffList) {
                damage += buff.infomation.energyPower;
            }

            foreach (Ability equipment in equipments) {
                damage += equipment.energyPower;
            }

            attchTarget.hit(damage);
            delay(infomation.afterDelay, "backStatus");
        }
    }

    private void backStatus() {
        status = prevStatus;
    } // 이전상태로 돌아감

    private void delay(float time, string callBack) {
        foreach (StatusBar bar in delayBar) {
            bar.setMaximum(time);
            bar.runProgress();
        }
        
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
            dead();            
        }
    } // 공격받음

    private void dead() {
        Destroy(this.gameObject);
    }

    private void clear() {
        this.gameObject.GetComponent<Image>().color = new Color(255, 255, 255);
    }

    public void move() {
        Vector2 currentPosition = this.gameObject.transform.position;

        status = (target == null && status != (int)CharacterStatus.Control) ? (int)CharacterStatus.Normal : status;

        if (status == (int)CharacterStatus.Battle && target != null) {
            float distance = Vector2.Distance(target.transform.position, currentPosition);

            if (this.infomation.range >= distance) {
                this.prevStatus = status;
                this.status = (int)CharacterStatus.Attack;
                beforeDelayActionStr = "attch";

                foreach (StatusBar bar in delayBar) {
                    bar.setMaximum(infomation.beforeDelay);
                }
                delay(infomation.beforeDelay, beforeDelayActionStr);
            }

            if (this.infomation.aggroRadius < distance) {
                target = null;
                status = (int)CharacterStatus.Normal;
            } // 어그로 해제
        }

        this.gameObject.transform.position = new Vector2(currentPosition.x + (direction * infomation.movementSpeed), currentPosition.y);
        
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
        if (this.infomation.aggroRadius >= distance) {
            returnData = true;
        }

        return returnData;
    } // 대상탐색 - 대상이 어그로 범위 안에 있는지 체크합니다.
}
