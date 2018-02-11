using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero {
    public string name;
    public int level;
    public int jobClass;
    public int passive;
    public int mainSkill;
    public int sideSkill;
    public bool teamOrder;

    public Hero(string setName, int setLevel, int setJobClass, int setPassive,
        int setMainSkill, int setSideSkill, bool setTeamOrder) {
        name = setName;
        level = setLevel;
        jobClass = setJobClass;
        passive = setPassive;
        mainSkill = setMainSkill;
        teamOrder = setTeamOrder;
    }
}
