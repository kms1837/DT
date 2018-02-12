using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour {
    public GameObject HeroGroup;
    public GameObject EnemyGroup;
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

	// Update is called once per frame
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
