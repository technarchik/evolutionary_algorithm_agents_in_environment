using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Environment : MonoBehaviour
{
    //[Header("Easy Mode Range")]
    //[SerializeField] float easyMin;
    //[SerializeField] float easyMax;

    //[Header("Medium Mode Range")]
    //[SerializeField] float mediumMin;
    //[SerializeField] float mediumMax;

    //[Header("Hard Mode Range")]
    //[SerializeField] float hardMin;
    //[SerializeField] float hardMax;

    [Header("Generations")]
    public int currentGenerationInEnv = 0;
    public int generationMax = 50;
    public int era = 0;

    public static event Action OnEnvironmentChanged;

    public enum DifficultyMode
    {
        easyMode, mediumMode, hardMode
    }
    public DifficultyMode difficultyMode;

    public float temp;
    public float wet;
    public float windStrength;
    public float eatPredator;
    public float eatHerbivore;


    public float TempCalc(float temp, float wet, float windStrength)
    {
        temp = 37 - ((37 - temp) / (0.68f - 0.0014f * wet + (1 / (1.76f + 1.4f * Mathf.Pow(windStrength, 0.75f)))));       // из формулы эквивалентно-эффективной температуры - ПРОВЕРИТЬ ЕЩЕ АДЕКВАТНОСТЬ ЦИФР
        return temp;
    }

    public void Initialize()
    {
        if (era != 0)
        {
            // set difficulty with probability
            Debug.Log("Env changed");
            float random = UnityEngine.Random.value;   // from 0 to 1

            if (random < 0.5f)
                difficultyMode = DifficultyMode.easyMode;         // 50%
            else if (random < 0.7f)
                difficultyMode = DifficultyMode.mediumMode;       // 30%
            else
                difficultyMode = DifficultyMode.hardMode;         // 20%

            GenerateConditions();
            temp = TempCalc(temp, wet, windStrength);
        }
        else if (era == 0)
        {
            difficultyMode = DifficultyMode.easyMode;
            GenerateConditions();
            temp = TempCalc(temp, wet, windStrength);
        }
    }

    public void GenerateConditions()
    {
        switch (difficultyMode)
        {
            case DifficultyMode.easyMode:
                temp = UnityEngine.Random.Range(10, 20);        // если че, можно еще UnityEngine.Random.Range
                wet = UnityEngine.Random.Range(40, 55);
                windStrength = UnityEngine.Random.Range(0, 2);
                eatPredator = UnityEngine.Random.Range(50, 70);        // мб здесь тоже в зависимости от ветра и всего остального генерить диапазон? - ИЛИ ЗАБИТЬ??
                eatHerbivore = UnityEngine.Random.Range(50, 70);
                break;
            case DifficultyMode.mediumMode:
                temp = UnityEngine.Random.Range(-10, 30);
                wet = UnityEngine.Random.Range(30, 70);
                windStrength = UnityEngine.Random.Range(3, 9);
                eatPredator = UnityEngine.Random.Range(40, 60);
                eatHerbivore = UnityEngine.Random.Range(40, 60);
                break;
            case DifficultyMode.hardMode:
                temp = UnityEngine.Random.Range(-30, 50);
                wet = UnityEngine.Random.Range(20, 100);
                windStrength = UnityEngine.Random.Range(10, 18);
                eatPredator = UnityEngine.Random.Range(20, 40);
                eatHerbivore = UnityEngine.Random.Range(30, 50);
                break;
        }
        OnEnvironmentChanged?.Invoke();
    }

    public void NextGeneration()
    {
        currentGenerationInEnv++;
        OnEnvironmentChanged?.Invoke();

        if (currentGenerationInEnv >= generationMax)
            StartNewEra();
    }

    void StartNewEra()
    {
        era++;
        currentGenerationInEnv = 0;
        Initialize();
        OnEnvironmentChanged?.Invoke();
    }
}
