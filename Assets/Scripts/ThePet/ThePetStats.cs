using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePetStats : EntityStats<ThePetStats>
{
    //---------------【基础属性】-----------------//
    public float intimacy = 30.0f;//亲密度
    public float happiness = 43.0f;//开心值                 
    public float energy = 71.0f;//当前活力值
    public float energy_max = 100.0f;//活力值上限
    public float focus = 100.0f;//专注值
    public float satiety = 100.0f;//饱食度
}
