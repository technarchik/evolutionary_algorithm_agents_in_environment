using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : Animal
{
    public float huntAbility
    {
        get { return speed + stamina; } // maybe multipluy? #todo
    }

    private RandomMovement randomMovement;
    private Herbivore currentPrey;
    private Vector3 huntDirection;
    private bool isHunting = false;

    private void Awake()
    {
        randomMovement = GetComponent<RandomMovement>();
    }

    private void Update()
    {
        if (isHunting && currentPrey != null)
        {
            // move toward
            transform.position = Vector3.MoveTowards(transform.position, currentPrey.transform.position, huntAbility * Time.deltaTime);
        }
    }

    public void Kill(Herbivore prey)
    {
        if (prey == null) return;

        currentPrey = prey;
        isHunting = true;

        // pause random moving
        if (randomMovement != null)
            randomMovement.PauseWalking();

        // vector of movement toward the prey
        huntDirection = (prey.transform.position - transform.position).normalized;

        // send a direction to the prey
        prey.Run(huntDirection);
    }

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
