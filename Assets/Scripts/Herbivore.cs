using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbivore : Animal
{
    public float escapeAbility
    {
        get { return speed + stamina; } // maybe multipluy? #todo
    }

    // у травоядного пока нет способности к запасу (а-ля настолка Эволюция - у животного нет фишки жира)
    // просто иначе как ему в хардовом режиме вообще выжить? у хищника хотя бы охота есть
}
