using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public float hp;
    public float speed;
    public float stamina;

    public float temp_resist;
    public float wet_resist;
    public float eat_need = 1.2f;       // смотреть в зависимости от количества генериремой еды в текущем режиме

    public virtual List<float> GetCharacteristics()
    {
        return new List<float>
        {
            hp,
            speed,
            stamina,
            temp_resist
        };
    }
}

public class Predator : Animal
{
    public float hunt_ability;

    public void Kill(Herbivore prey) // либо список из Herbivore передаем
    {
        /* 
        Какой-то из 4 вариантов:

        1) Берем рандомных хищников и рандомных травоядных.
        - могут неравномерно развиваться те или другие.

        2) Объединяем в одно среднее (ср. темпа по больнице среди каждого вида)
        - будут выбросы, котороые могут сломать, но если брать не сред. ариф. мб прокатит.

        3) Брать лучшего из охотников и охотиться на лучшего из травоядных?
        - типа блин проверяем выражение 1 = 1??
        
        4) Пройтись по всем хищникам и в зависимости от того, кто преуспел - тому дать бонус к ... фитнесу или бонус в отдельное поле этого хищника, которое будет проверяться в фитнесе?
        А кого едим?
        Среднестатистическое травоядное текущей генерации?
        Но так не дебафаем ли развитие травоядных? Хотя тут надо просто смотреть, как распределены силы при проигрыше/выигрыше в гонке.
        
         */
    }
}

public class Herbivore : Animal
{
    public float escape_ability;
    // у травоядного пока нет способности к запасу (а-ля настолка Эволюция - у животного нет фишки жира)
    // просто иначе как ему в хардовом режиме вообще выжить? у хищника хотя бы охота есть
}
