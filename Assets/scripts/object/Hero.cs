using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Ability {
    public Hero(string setName, int setLevel, int setJobClass, int setPassive,
        int setMainSkill, int setSideSkill) {
        nameStr = setName;
        level = setLevel;
        jobClass = setJobClass;
        passive = setPassive;
        mainSkill = setMainSkill;
    }
}
