using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour {

    public string name;

    public int status; // 케릭터 상태
    private int prevStatus; // 전 상태
    public int type; // 케릭터 타입

    // status
    public int level;
    public float healthPoint; // HP
    public float manaPoint; // MP

    public float energyPower; // 기력
    public float magicPower; // 마력
    public float healthPower; // 체력
    public float holyPower; // 신성력

    public float attackSpeed; // 공격속도
    public float movementSpeed; // 이동속도
    public float range; // 공격거리

    public float beforeDelay; // 선딜레이
    public float afterDelay; // 후딜레이

    public GameObject target; // 공격대상

    public int direction; // 방향
    public float aggroRadius; // 인식 거리

    public enum CharacterStatus { Normal, Battle, Attack }
    // 평상시, 전투모드, 공격중

    public enum CharacterType { Hero, NPC, Monster, Boss }

    private string beforeDelayActionStr; // 선딜레이 중인 함수 이름(invoke 중인 상태)

    // Use this for initialization
    void Start () {
        Rect objRect = this.gameObject.GetComponent<RectTransform>().rect;

        status = (int)CharacterStatus.Normal;

        target = null;

        movementSpeed = 2;
        direction = 1;
        beforeDelay = 2.0f;
        energyPower = 10;
        healthPoint = 100;

        aggroRadius = objRect.width + 200;
        range = objRect.width + 10;

        beforeDelayActionStr = string.Empty;
    }
	
	// Update is called once per frame
	void Update () {
		
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
            attchTarget.hit(energyPower);
            Invoke("backStatus", afterDelay);
        }
    }

    private void backStatus() {
        status = prevStatus;
    } // 이전상태로 돌아감

    public void hit(float damage) {
        this.gameObject.GetComponent<Image>().color = new Color(255, 0, 0);
        Invoke("clear", 0.1f);

        if (beforeDelayActionStr != string.Empty) {
            CancelInvoke(beforeDelayActionStr);
            beforeDelayActionStr = string.Empty;
            status = (int)CharacterStatus.Battle;
        }
        
        healthPoint -= damage;
        if (healthPoint <= 0) {
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
                Invoke(beforeDelayActionStr, beforeDelay);
            }

            if (this.aggroRadius < distance) {
                target = null;
                status = (int)CharacterStatus.Normal;
            } // 어그로 해제
        }

        this.gameObject.transform.position = new Vector2(currentPosition.x + (direction * movementSpeed), currentPosition.y);
        
    } // 이동

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
