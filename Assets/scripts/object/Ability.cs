using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public int id; // 케릭터 id
    public string nameStr; // 이름(닉네임)

    public int level;

    public int jobClass; // 직업

    /* total ability */
    public float healthPoint; // HP
    public float manaPoint; // MP

    public float defencePoint; // 방어력

    public float beforeDelay; // 선딜레이
    public float afterDelay; // 후딜레이
    
    public float movementSpeed; // 이동속도
    public float range; // 공격거리

    public float power;

    /* normal ability */
    public float energyPower; // 기력
    public float magicPower; // 마력
    public float healthPower; // 체력
    public float holyPower; // 신성력

    /* result ability */
    public float phy; // 물리력
    public float mag; // 마법력
    public float ap;
    public float heal; // 회복력

    public float aggroRadius; // 인식 거리


    // skill Number
    public int passive;
    public int mainSkill;
    public int sideSkill;

    public void cloneAbility(Ability cloneObj) {
        cloneObj.nameStr = this.nameStr;

        cloneObj.level = this.level;
        cloneObj.healthPoint = this.healthPoint;

        cloneObj.manaPoint = this.manaPoint;

        cloneObj.energyPower = this.energyPower;
        cloneObj.magicPower = this.magicPower;
        cloneObj.healthPower = this.healthPower;
        cloneObj.holyPower = this.holyPower;
        
        cloneObj.movementSpeed = this.movementSpeed;
        cloneObj.range = this.range;

        cloneObj.beforeDelay = this.beforeDelay;
        cloneObj.afterDelay = this.afterDelay;

        cloneObj.aggroRadius = this.aggroRadius;
    }

    public Ability () {
        nameStr = "";

        level = 1;
        healthPoint = 0; // HP
        manaPoint = 0; // MP

        jobClass = 1;

        passive = 1;
        mainSkill = 1;
        sideSkill = 1;

        energyPower = 0; // 기력
        magicPower = 0; // 마력
        healthPower = 0; // 체력
        holyPower = 0; // 신성력
        
        movementSpeed = 0; // 이동속도
        range = 0; // 공격거리

        beforeDelay = 0; // 선딜레이
        afterDelay = 0; // 후딜레이

        aggroRadius = 0; // 인식 거리

    }
}
