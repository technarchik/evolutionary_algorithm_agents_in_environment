using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [Header("Easy Mode Range")]
    [SerializeField] float easy_min;
    [SerializeField] float easy_max;

    [Header("Medium Mode Range")]
    [SerializeField] float medium_min;
    [SerializeField] float medium_max;

    [Header("Hard Mode Range")]
    [SerializeField] float hard_min;
    [SerializeField] float hard_max;

    public enum DifficultyMode
    {
        easyMode, mediumMode, hardMode
    }
    public DifficultyMode difficultyMode;

    public float temp;
    public float wet;
    public float wind_strength;
    public float eat_predator;
    public float eat_herbivore;

    public int generation_now = 0;
    public int generation_max = 50;

    public float TempCalc(float temp, float wet, float wind_strength)
    {
        temp = 37 - ((37 - temp) / (0.68f - 0.0014f * wet + (1 / (1.76f + 1.4f * Mathf.Pow(wind_strength, 0.75f)))));       // из формулы эквивалентно-эффективной температуры - ПРОВЕРИТЬ ЕЩЕ АДЕКВАТНОСТЬ ЦИФР
        return temp;
    }

    public void Initialize()
    {
        if (generation_now != 0)
        {
            // Определяем сложность с вероятностями
            float r = Random.value;   // от 0 до 1

            if (r < 0.7f)
                difficultyMode = DifficultyMode.easyMode;         // 70%
            else if (r < 0.9f)
                difficultyMode = DifficultyMode.mediumMode;       // 20%
            else
                difficultyMode = DifficultyMode.hardMode;         // 10%

            GenerateConditions();
            temp = TempCalc(temp, wet, wind_strength);
        }
    }

    public void GenerateConditions()
    {
        switch (difficultyMode)
        {
            case DifficultyMode.easyMode:
                temp = Random.Range(10, 20);        // если че, можно еще UnityEngine.Random.Range
                wet = Random.Range(40, 55);
                wind_strength = Random.Range(0, 2);
                eat_predator = Random.Range(50, 70);        // мб здесь тоже в зависимости от ветра и всего остального генерить диапазон? - ИЛИ ЗАБИТЬ??
                eat_herbivore = Random.Range(50, 70);
                break;
            case DifficultyMode.mediumMode:
                temp = Random.Range(-10, 30);
                wet = Random.Range(30, 70);
                wind_strength = Random.Range(3, 9);
                eat_predator = Random.Range(40, 60);
                eat_herbivore = Random.Range(40, 60);
                break;
            case DifficultyMode.hardMode:
                temp = Random.Range(-30, 50);
                wet = Random.Range(20, 100);
                wind_strength = Random.Range(10, 18);
                eat_predator = Random.Range(20, 40);
                eat_herbivore = Random.Range(20, 40);
                break;
        }
    }

    
}
