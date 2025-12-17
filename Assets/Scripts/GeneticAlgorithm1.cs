using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneticAlgorithm1 : MonoBehaviour
{
    [Header("References")]
    public Environment env;
    public GameObject predatorPrefab;
    public GameObject herbivorePrefab;

    [Header("Population sizes")]
    public int predatorCount = 25;
    public int herbivoreCount = 25;

    [Header("Generations")]
    public int generationPerEnv = 50;
    public int currentGenerationInEnv = 0;

    [Header("GA parameters")]
    [Range(0f, 1f)] public float mutationRate = 0.05f;
    [Range(0f, 1f)] public float crossoverRate = 0.9f;
    public int elitismCount = 2;
    public int tournamentSize = 3;

    [Header("Initial ranges")]
    public TraitRange staminaRange = new TraitRange(5f, 20f);
    public TraitRange speedRange = new TraitRange(0.5f, 5f);
    public TraitRange tempResistRange = new TraitRange(0f, 1f);
    public TraitRange wetResistRange = new TraitRange(0f, 1f);
    public TraitRange eatNeedRange = new TraitRange(0.5f, 2f);
     
    // 2 populations that keep predators and herbivores separately
    private List<Predator> predators = new List<Predator>();
    private List<Herbivore> herbivores = new List<Herbivore>();



    private void Start()
    {
        
    }


    #region Initialization

    void InitializeEnvironment()
    {
        env.Initialize();
        currentGenerationInEnv = 0;
    }

    void InitializePopulations()
    {
        foreach (var p in predators)
            Destroy(p.gameObject);
        foreach (var h in herbivores)
            Destroy(h.gameObject);
        predators.Clear();
        herbivores.Clear();

        for (int i = 0; i < predatorCount;  i++)
        {
            var createPredatorObject = Instantiate(predatorPrefab);
            var predatorObject = createPredatorObject.GetComponent<Predator>();
            if (predatorObject == null) predatorObject.AddComponent<Predator>();
            CreateGenes(predatorObject);
            predators.Add(predatorObject);
        }
    }

    void CreateGenes(Animal animal)
    {
        animal.hp = 100f;
        animal.stamina = Random.Range(staminaRange.min, staminaRange.max);
        animal.speed = Random.Range(speedRange.min, speedRange.max);
        animal.temp_resist = Random.Range(tempResistRange.min, tempResistRange.max);
        animal.wet_resist = Random.Range(wetResistRange.min, wetResistRange.max);
        animal.eat_need = Random.Range(eatNeedRange.min, eatNeedRange.max);
    }

    #endregion


    #region GA algorithm

    #endregion


    #region Fitness function

    #endregion


    #region Crossover algorithm

    #endregion


    #region Mutation algorithm

    #endregion


    #region Sort algorithm

    #endregion


    #region Elitism algorithm

    #endregion


    #region Helpers
    // [System.Serializable]
    public struct TraitRange        // struct for applying intervals
    {
        public float min, max;
        public TraitRange(float min, float max) 
        { 
            this.min = min; 
            this.max = max; 
        }
    }
    #endregion
}


