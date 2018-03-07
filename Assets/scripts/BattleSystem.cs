using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour {
    public GameObject HeroGroup;
    public GameObject EnemyGroup;
    public GameObject User; // 조작 하는 케릭터

    public GameObject BottomView;

    private StatusBar skillCollTimeBar;

    // Use this for initialization
    void Start () {
        int temp = 100;
        foreach (Transform heroObj in HeroGroup.transform) {
            Character hero = heroObj.GetComponent<Character>();
            hero.range += temp;
            temp += 100;
        }

        temp = 50;
        foreach (Transform enemyObj in EnemyGroup.transform) {
            Character enemy = enemyObj.GetComponent<Character>();
            enemy.direction = -1;
            enemy.range += temp;
            temp += 100;
        }

        Character UserObj = User.GetComponent<Character>();
        Skill testSkill = User.AddComponent<Skill>();

        UserObj.mainSkillObj = testSkill;

        testSkill.targetList.Add(UserObj);
        testSkill.type = (int)Skill.SkillType.Holy;
        //testSkill.type = (int)Skill.SkillType.Buff;
        testSkill.healthPoint = 20;
        testSkill.energyPower = 50;
        testSkill.duration = 300;
        testSkill.coolTime = 5;

        Transform mainSkillBtn = BottomView.transform.Find("MainSkillButton");

        skillCollTimeBar = mainSkillBtn.Find("CoolTimeBar").GetComponent<StatusBar>();
        skillCollTimeBar.init(testSkill.coolTime, new Color(255.0f, 100, 0));
    }

    public void ActiveUserMainSkill () {
        Character UserObj = User.GetComponent<Character>();
        if (UserObj.ActiveMainSkill()) {
            Skill testSkill = User.GetComponent<Skill>();
            skillCollTimeBar.runProgress(testSkill.coolTime);
        };
    }

    public void ActiveUserSubSkill() {
        Character UserObj = User.GetComponent<Character>();
        UserObj.ActiveSubSkill();
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
    
	void Update () {
        foreach (Transform heroObj in HeroGroup.transform) {
            Hero hero = heroObj.GetComponent<Hero>();

            switch (hero.status) {
                case (int)Hero.CharacterStatus.Normal:
                    hero.move();
                    searchEnemy(heroObj, EnemyGroup.transform);
                    break;
                case (int)Hero.CharacterStatus.Battle:
                    hero.move();
                    break;
                case (int)Hero.CharacterStatus.Attack:
                    break;
            }
        } // hero pattern

        foreach (Transform enemyObj in EnemyGroup.transform) {
            Character enemy = enemyObj.GetComponent<Character>();

            switch (enemy.status) {
                case (int)Hero.CharacterStatus.Normal:
                    enemy.move();
                    searchEnemy(enemyObj, HeroGroup.transform);
                    break;
                case (int)Hero.CharacterStatus.Battle:
                    enemy.move();
                    break;
                case (int)Hero.CharacterStatus.Attack:
                    break;
            }
        } // enemy pattern
    }
}
