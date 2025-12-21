using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public float hp;
    public float stamina;

    // поле, характерное для взаимодействия агентов друг с другом
    public float speed;

    // поля, характерные для устойчивостей условиям окружающей среды
    public float tempResist;
    public float wetResist;
    public float eatNeed = 1.2f;       // смотреть в зависимости от количества генерируемой еды в текущем режиме
                                        // оно меняется в ГА, но нужно будет задать предельные интервалы, в которых оно будет меняться, чтобы не перебустить животное

    // фитнес агента
    public float score;

    [SerializeField] private TMP_Text infoText; 

    public virtual List<float> GetCharacteristics()
    {
        return new List<float>
        {
            hp,
            stamina,
            speed,
            tempResist,
            wetResist,
            eatNeed
        };
    }

    public void UpdateUI()
    {
        infoText.text =
        $"HP: {hp}\n" +
        $"Stamina: {stamina:F2}\n" +
        $"Speed: {speed:F2}\n" +
        $"Temp: {tempResist:F2}\n" +
        $"Wet: {wetResist:F2}\n" +
        $"Eat: {eatNeed:F2}";
    }
}
