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

    public ArrayList targetBuffList = new ArrayList();
    public ArrayList targetList = new ArrayList();

    public enum SkillType { Buff, Magic, Physical, Holy }

    public bool activation() {
        if (!activeFlag) {
            Debug.Log("coolTime: " + coolTime);
            return false;
        }

        activeFlag = false;

        Debug.Log("발동!");

        switch (type) {
            case (int)SkillType.Buff:
                foreach (Character target in targetList) {
                    Skill newBuff = new Skill();
                    this.cloneAbility(newBuff);
                    target.buffList.Add(buffObj);

                    newBuff.Invoke("buffRelease", duration);
                } // issue!

                /*
                if (buffObj == null) {
                    buffObj = new Skill();
                    this.cloneAbility(buffObj);
                    targetBuffList.Add(buffObj);
                    // issue - 버프대상이 많을경우? autoRelease를 만들어야 겠음
                } else {
                    this.CancelInvoke("buffRelease");
                }
                this.Invoke("buffRelease", duration);
                */
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
