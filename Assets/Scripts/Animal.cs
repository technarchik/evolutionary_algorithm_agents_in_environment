using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public float hp;
    public float stamina;

    // поле, характерное дл€ взаимодействи€ агентов друг с другом
    public float speed;

    // пол€, характерные дл€ устойчивостей услови€м окружающей среды
    public float temp_resist;
    public float wet_resist;
    public float eat_need = 1.2f;       // смотреть в зависимости от количества генерируемой еды в текущем режиме

    // фитнес агента
    public float score;
    
    public virtual List<float> GetCharacteristics()
    {
        return new List<float>
        {
            hp,
            stamina,
            speed,
            temp_resist,
            wet_resist,
            eat_need
        };
    }

}

public class Predator : Animal
{
    public float hunt_ability
    {
        get { return speed + stamina; }
    }

    public void Kill(Herbivore prey) // либо список из Herbivore передаем
    {
        /* 
         акой-то из 4 вариантов:

        1) Ѕерем рандомных хищников и рандомных траво€дных.
        - могут неравномерно развиватьс€ те или другие.

        2) ќбъедин€ем в одно среднее (ср. темпа по больнице среди каждого вида)
        - будут выбросы, котороые могут сломать, но если брать не сред. ариф. мб прокатит.

        3) Ѕрать лучшего из охотников и охотитьс€ на лучшего из траво€дных?
        - типа блин провер€ем выражение 1 = 1??
        
        4) ѕройтись по всем хищникам и в зависимости от того, кто преуспел - тому дать бонус к ... фитнесу или бонус в отдельное поле этого хищника, которое будет провер€тьс€ в фитнесе?
        ј кого едим?
        —реднестатистическое траво€дное текущей генерации?
        Ќо так не дебафаем ли развитие траво€дных? ’от€ тут надо просто смотреть, как распределены силы при проигрыше/выигрыше в гонке.
        
         */
    }
}

public class Herbivore : Animal
{
    public float escape_ability
    {
        get { return speed + stamina; }
    }

    // у траво€дного пока нет способности к запасу (а-л€ настолка Ёволюци€ - у животного нет фишки жира)
    // просто иначе как ему в хардовом режиме вообще выжить? у хищника хот€ бы охота есть
}
