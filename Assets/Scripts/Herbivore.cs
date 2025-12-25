using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbivore : Animal
{
    public float escapeAbility
    {
        get { return speed + stamina; } // maybe multipluy? or mb normilize stamina and: speed + (staminaN * speed)     #todo
        // get { return speed + speed * (stamina / 20f); }
    }

    private RandomMovement randomMovement;
    private bool isEscaping = false;
    private Vector3 escapeDirection;
    private void Awake()
    {
        randomMovement = GetComponent<RandomMovement>();
    }
    private void Update()
    {
        if (isEscaping)
        {
            transform.position += escapeDirection * escapeAbility * Time.deltaTime;
        }
    }

    // run from the predator!
    public void Run(Vector3 predatorDirection)
    {
        isEscaping = true;

        // pause random movement
        if (randomMovement != null)
            randomMovement.PauseWalking();

        // start escaping
        escapeDirection = -predatorDirection.normalized;
    }

    // check if this is prey
    public bool Listen(Herbivore prey)
    {
        return prey == this;
    }

    // у травоядного пока нет способности к запасу (а-ля настолка Эволюция - у животного нет фишки жира)
    // просто иначе как ему в хардовом режиме вообще выжить? у хищника хотя бы охота есть
}
