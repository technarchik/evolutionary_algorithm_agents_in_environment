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


    #region Natural Selection

    #endregion


    #region GA algorithm
    // method for evolving classes from Animal - doesnt matter what type
    List<List<float>> EvolveAnimals<T>(List<T> animals) where T : Animal 
    {
        // fitness (before it) -> sorting -> elitism -> crossover -> mutation
        // sorting
        animals.Sort((a, b) => b.score.CompareTo(a.score));
        List<List<float>> children = new List<List<float>>();

        // elitism
        for (int i = 0; i < elitismCount; i++)
        {
            children.Add(TakeGenesFromAnimal(animals[i]));
        }

        // crossover (needed tournament selection)
        while (children.Count < animals.Count)
        {
            // selecting parents
            Animal mother = TournamentSelection(animals);
            Animal father = TournamentSelection(animals);

            // remember their genes
            var genesMother = TakeGenesFromAnimal(mother);
            var genesFather = TakeGenesFromAnimal(father);

            // crossover mechanism
            if (Random.value < crossoverRate)
                CrossoverAlgorithm(genesMother, genesFather);

            // mutation mechanism
            Mutation(genesMother);
            Mutation(genesFather);

            // adding genes to the next generation
            children.Add(genesMother);
            if (children.Count < animals.Count)    // idk should i do this way?
                children.Add(genesFather);
        }
        return children;
    }

    List<float> TakeGenesFromAnimal(Animal animal)
    {
        List<float> geneList = new List<float> { animal.stamina, animal.speed, animal.temp_resist, animal.wet_resist, animal.eat_need};
        return geneList;
    }

    Animal TournamentSelection<T>(List<T> list) where T : Animal
    {
        Animal bestAnimal = null;
        // may be can write this method another way
        for (int i = 0; i < tournamentSize; i++) 
        {
            var c = list[Random.Range(0, list.Count)];
            if (bestAnimal == null || c.score > bestAnimal.score)
                bestAnimal = c;
        }
        return bestAnimal;
    }

    #endregion


    #region Fitness functions

    void EvaluateFitnessPredator()
    {
        foreach (var predator in predators)
        {
            // must count, what important for predators?
        }
    }

    void EvaluateFitnessHerbivore()
    {
        foreach(var herbivore in herbivores)
        {
            // must count, what important for herbivores?
        }
    }

    void EvaluateFitnessInEnvironment(Animal animal)
    {
        float tempFit;
        float wetFit;
    }
    #endregion


    #region Crossover algorithm

    void CrossoverAlgorithm(List<float> mother, List<float> father)
    {
        // VARIANT 1: single point crossover
        int point = Random.Range(1, mother.Count);
        for (int i = point; i < mother.Count; i++)
        {
            float temp = mother[i];
            mother[i] = father[i];
            father[i] = temp;
        }
    }

    #endregion


    #region Mutation algorithm

    void Mutation(List<float> gene)
    {
        for (int i = 0; i < gene.Count; i++)
        {
            if (Random.value < mutationRate)
            {
                // VARIANT 1: without checking allowed interval
                // gene[i] += Random.Range(-0.1f, 0.1f); 

                // VARIANT 2: with checking allowed interval
                float noise = Random.Range(-0.1f, 0.1f);
                gene[i] += noise;

                switch(i)
                {
                    case 0: // stamina
                        gene[i] = Mathf.Clamp(gene[i], staminaRange.min, staminaRange.max);
                        break;
                    case 1: // speed
                        gene[i] = Mathf.Clamp(gene[i], speedRange.min, speedRange.max);
                        break;
                    case 2: // temp_resist
                        gene[i] = Mathf.Clamp(gene[i], tempResistRange.min, tempResistRange.max);
                        break;
                    case 3: // wet_resist
                        gene[i] = Mathf.Clamp(gene[i], wetResistRange.min, wetResistRange.max);
                        break;
                    case 4: //eat_need
                        gene[i] = Mathf.Clamp(gene[i], eatNeedRange.min, eatNeedRange.max);
                        break;

                }

                // VARIANT 3: mutaion adapting to difficultyMode of the env
                // float envFactor = env.difficultyMode == Environment.DifficultyMode.hardMode ? 0.2f : 0.05f;
                // float delta = Random.Range(-envFactor, envFactor);
            }
        }
    }

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


