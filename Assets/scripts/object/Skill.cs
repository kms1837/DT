using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : Ability
{
    public int type; // 스킬 타입
    public float duration; // 스킬 지속 시간
    public float coolTime; // 스킬 재사용 대기시간

    private bool activeFlag; // 스킬 사용 가능 여부
    private Skill buffObj;

    public ArrayList targetBuffList;
    public ArrayList targetList;

    public enum SkillType { Buff, Magic, Physical, Holy }

    public bool activation() {
        if (!activeFlag) {
            return false;
        }

        switch (type) {
            case (int)SkillType.Buff:
                if (buffObj == null) {
                    buffObj = new Skill();
                    this.cloneAbility(buffObj);
                    targetBuffList.Add(buffObj);
                } else {
                    this.CancelInvoke("buffRelease");
                }
                this.Invoke("buffRelease", duration);
                break;

            case (int)SkillType.Magic:
                // 투사체 발사
                break;

            case (int)SkillType.Physical:
                // 공격하고 적에게 이펙트 재생
                break;

            case (int)SkillType.Holy:
                foreach(Character target in targetList) {
                    float heal = target.currentHealthPoint + this.healthPoint;
                    target.currentHealthPoint = heal <= target.healthPoint ? heal : target.healthPoint;
                }
                break;
        }

        activeFlag = false;

        this.Invoke("recycle", coolTime);

        return true;
    } // 스킬 발동

    private void recycle() {
        activeFlag = true;
    }

    public void buffRelease() {
        targetBuffList.Remove(buffObj);
        buffObj = null;
    } // 버프를 해제합니다.

    private void Start () {
        buffObj = null;
        activeFlag = true;
    }
}
