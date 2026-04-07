using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Difficulty Settings")]
    [SerializeField] private bool dynamic = true;
    public enum DifficultyMode
    {
        easyMode, mediumMode, hardMode
    }
    public DifficultyMode difficultyMode;

    [Header("Current Conditions")]
    public float temp;
    public float wet;
    public float windStrength;
    public float eatPredator;
    public float eatHerbivore;

    [Header("Conditions Ranges")]
    // easyMode
    public TraitRange temp_Range_Easy = new(10, 20);
    public TraitRange wet_Range_Easy = new(40, 55);
    public TraitRange wind_Range_Easy = new(0, 2);
    public TraitRange eatPred_Range_Easy = new(4, 7);
    public TraitRange eatHerb_Range_Easy = new(4, 7);
    // mediumMode
    public TraitRange temp_Range_Medium = new(-10, 30);
    public TraitRange wet_Range_Medium = new(30, 70);
    public TraitRange wind_Range_Medium = new(3, 9);
    public TraitRange eatPred_Range_Medium = new(2, 6);
    public TraitRange eatHerb_Range_Medium = new(2, 6);
    // hardMode
    public TraitRange temp_Range_Hard = new(-30, 50);
    public TraitRange wet_Range_Hard = new(20, 100);
    public TraitRange wind_Range_Hard = new(10, 18);
    public TraitRange eatPred_Range_Hard = new(1, 3);
    public TraitRange eatHerb_Range_Hard = new(2, 4);

    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "SceneGameSimulation")
            Initialize();
    }

    public float TempCalc(float temp, float wet, float windStrength)
    {
        temp = 37 - ((37 - temp) / (0.68f - 0.0014f * wet + (1 / (1.76f + 1.4f * Mathf.Pow(windStrength, 0.75f)))));       // из формулы эквивалентно-эффективной температуры - ПРОВЕРИТЬ ЕЩЕ АДЕКВАТНОСТЬ ЦИФР
        return temp;                                                                                                       // додбавить еще последнюю часть!
    }

    public void Initialize()
    {
        if (dynamic)
        {
            // DYNAMIC: set difficulty with probability
            if (era != 0)
            {
                float random = UnityEngine.Random.value;   // from 0 to 1

                if (random < 0.5f)
                    difficultyMode = DifficultyMode.easyMode;         // 50%
                else if (random < 0.7f)
                    difficultyMode = DifficultyMode.mediumMode;       // 30%
                else
                    difficultyMode = DifficultyMode.hardMode;         // 20%
            }
            else if (era == 0)
            {
                difficultyMode = DifficultyMode.easyMode;
            }
        }
        else
        {
            // STATIC: difficultyMode is taken from the inspector
        }

        GenerateConditions();
        temp = TempCalc(temp, wet, windStrength);

        IsInitialized = true;

        Debug.Log($"Env changed!    temp: {temp} | wet: {wet} | wind: {windStrength} | eat predator: {eatPredator} | eat herbivore: {eatHerbivore}");
    }

    public void GenerateConditions()
    {
        switch (difficultyMode)
        {
            case DifficultyMode.easyMode:
                temp =          UnityEngine.Random.Range(temp_Range_Easy.min, temp_Range_Easy.max);        // если че, можно еще UnityEngine.Random.Range
                wet =           UnityEngine.Random.Range(wet_Range_Easy.min, wet_Range_Easy.max);
                windStrength =  UnityEngine.Random.Range(wind_Range_Easy.min, wind_Range_Easy.max);
                eatPredator =   UnityEngine.Random.Range(eatPred_Range_Easy.min, eatPred_Range_Easy.max);
                eatHerbivore =  UnityEngine.Random.Range(eatHerb_Range_Easy.min, eatHerb_Range_Easy.max);
                break;
            case DifficultyMode.mediumMode:
                temp =          UnityEngine.Random.Range(temp_Range_Medium.min, temp_Range_Medium.max);
                wet =           UnityEngine.Random.Range(wet_Range_Medium.min, wet_Range_Medium.max);
                windStrength =  UnityEngine.Random.Range(wind_Range_Medium.min, wind_Range_Medium.max);
                eatPredator =   UnityEngine.Random.Range(eatPred_Range_Medium.min, eatPred_Range_Medium.max);
                eatHerbivore =  UnityEngine.Random.Range(eatHerb_Range_Medium.min, eatHerb_Range_Medium.max);
                break;
            case DifficultyMode.hardMode:
                temp =          UnityEngine.Random.Range(temp_Range_Hard.min, temp_Range_Hard.max);
                wet =           UnityEngine.Random.Range(wet_Range_Hard.min, wet_Range_Hard.max);
                windStrength =  UnityEngine.Random.Range(wind_Range_Hard.min, wind_Range_Hard.max);
                eatPredator =   UnityEngine.Random.Range(eatPred_Range_Hard.min, eatPred_Range_Hard.max);
                eatHerbivore =  UnityEngine.Random.Range(eatHerb_Range_Hard.min, eatHerb_Range_Hard.max);
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

    public void StartNewEra()
    {
        era++;
        currentGenerationInEnv = 0;
        Initialize();
        OnEnvironmentChanged?.Invoke();
    }

    // struct for applying intervals of characteristics
    public struct TraitRange
    {
        public float min, max;
        public TraitRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
