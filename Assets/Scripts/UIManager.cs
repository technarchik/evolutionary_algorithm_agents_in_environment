using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Environment environment;
    public GeneticAlgorithm1 geneticAlgorithm1;

    [Header("Text fields")]
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text tempText;
    [SerializeField] private TMP_Text wetText;
    [SerializeField] private TMP_Text windText;
    [SerializeField] private TMP_Text foodPredText;
    [SerializeField] private TMP_Text foodHerbText;
    [SerializeField] private TMP_Text generationText;
    [SerializeField] private TMP_Text era;                          // потом добавить здесь отображение!! #todo
    void Start()
    {
        
    }

    private void Update()
    {
        UpdateEnvironmentUI();
    }
    private void UpdateEnvironmentUI()                             // можно получше сделать с событием!! #todo
    {
        difficultyText.text = $"Difficulty: {environment.difficultyMode}";
        tempText.text = $"Temperature: {environment.temp:F1}";
        wetText.text = $"Humidity: {environment.wet:F1}%";
        windText.text = $"Wind: {environment.windStrength:F1}";
        foodPredText.text = $"Food (Predators): {environment.eatPredator:F0}";
        foodHerbText.text = $"Food (Herbivores): {environment.eatHerbivore:F0}";
        generationText.text =
            $"Generation: {environment.currentGenerationInEnv}/{environment.generationMax}";
    }
}
