using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [Header("Easy Mode Range")]
    [SerializeField] float easyMin;
    [SerializeField] float easyMax;

    [Header("Medium Mode Range")]
    [SerializeField] float mediumMin;
    [SerializeField] float mediumMax;

    [Header("Hard Mode Range")]
    [SerializeField] float hardMin;
    [SerializeField] float hardMax;

    [Header("Generations")]
    public int currentGenerationInEnv = 0;
    public int generationMax = 50;
    public int era = 0;

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

    
    private void Start()
    {
        
    }

    public float TempCalc(float temp, float wet, float windStrength)
    {
        temp = 37 - ((37 - temp) / (0.68f - 0.0014f * wet + (1 / (1.76f + 1.4f * Mathf.Pow(windStrength, 0.75f)))));       // из формулы эквивалентно-эффективной температуры - ПРОВЕРИТЬ ЕЩЕ АДЕКВАТНОСТЬ ЦИФР
        return temp;
    }

    public void Initialize()
    {
        if (currentGenerationInEnv != 0)
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
            temp = TempCalc(temp, wet, windStrength);
        }
        else
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
                temp = Random.Range(10, 20);        // если че, можно еще UnityEngine.Random.Range
                wet = Random.Range(40, 55);
                windStrength = Random.Range(0, 2);
                eatPredator = Random.Range(50, 70);        // мб здесь тоже в зависимости от ветра и всего остального генерить диапазон? - ИЛИ ЗАБИТЬ??
                eatHerbivore = Random.Range(50, 70);
                break;
            case DifficultyMode.mediumMode:
                temp = Random.Range(-10, 30);
                wet = Random.Range(30, 70);
                windStrength = Random.Range(3, 9);
                eatPredator = Random.Range(40, 60);
                eatHerbivore = Random.Range(40, 60);
                break;
            case DifficultyMode.hardMode:
                temp = Random.Range(-30, 50);
                wet = Random.Range(20, 100);
                windStrength = Random.Range(10, 18);
                eatPredator = Random.Range(20, 40);
                eatHerbivore = Random.Range(20, 40);
                break;
        }
    }
}
