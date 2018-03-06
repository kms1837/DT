using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {
    public string nameStr; // 이름

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

    public float aggroRadius; // 인식 거리

    public void cloneAbility(Ability cloneObj) {
        cloneObj.nameStr = this.nameStr;

        cloneObj.level = this.level;
        cloneObj.healthPoint = this.healthPoint;
        cloneObj.manaPoint = this.manaPoint;

        cloneObj.energyPower = this.energyPower;
        cloneObj.magicPower = this.magicPower;
        cloneObj.healthPower = this.healthPower;
        cloneObj.holyPower = this.holyPower;

        cloneObj.attackSpeed = this.attackSpeed;
        cloneObj.movementSpeed = this.movementSpeed;
        cloneObj.range = this.range;

        cloneObj.beforeDelay = this.beforeDelay;
        cloneObj.afterDelay = this.afterDelay;

        cloneObj.aggroRadius = this.aggroRadius;
    }

    void Start () {
        nameStr = "";

        level = 1;
        healthPoint = 0; // HP
        manaPoint = 0; // MP

        energyPower = 0; // 기력
        magicPower = 0; // 마력
        healthPower = 0; // 체력
        holyPower = 0; // 신성력

        attackSpeed = 0; // 공격속도
        movementSpeed = 0; // 이동속도
        range = 0; // 공격거리

        beforeDelay = 0; // 선딜레이
        afterDelay = 0; // 후딜레이

        aggroRadius = 0; // 인식 거리

    }
}
