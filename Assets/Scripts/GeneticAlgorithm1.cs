using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneticAlgorithm1 : MonoBehaviour
{
    public Transform predatorsPanel;
    public Transform herbivoresPanel;

    [Header("References")]
    public Environment env;
    public GameObject predatorPrefab;
    public GameObject herbivorePrefab;

    [Header("Population sizes")]
    public int predatorCount = 25;
    public int herbivoreCount = 25;

    [Header("GA parameters")]
    [Range(0f, 1f)] public float mutationRate = 0.05f;
    [Range(0f, 1f)] public float crossoverRate = 0.9f;
    public int firstDeltaHPLose = 10;
    public int secondDeltaHPLose = 20;
    public int elitismCount = 2;
    public int tournamentSize = 3;
    public int staminaUpdateInterval = 5;

    [Header("Initial ranges")]
    public TraitRange staminaRange = new TraitRange(5f, 20f);
    public TraitRange speedRange = new TraitRange(0.5f, 5f);
    public TraitRange tempResistRange = new TraitRange(0f, 1f);
    public TraitRange wetResistRange = new TraitRange(0f, 1f);
    public TraitRange eatNeedRange = new TraitRange(0.5f, 2f);
    public float noiseOfMutation = 0.1f;

    // 2 populations that keep predators and herbivores separately
    private List<Predator> predators = new List<Predator>();
    private List<Herbivore> herbivores = new List<Herbivore>();

    // another lists of genes for the next generation
    private List<List<float>> genesNextGenPredators = new List<List<float>>();
    private List<List<float>> genesNextGenHerbivores = new List<List<float>>();

    [Header("Simulation time")]
    [SerializeField] private float timeForOneGeneration = 1.0f; // how much seconds for one generation
    [SerializeField] private float simulationSpeed = 1.0f;      // multiply for speed

    private float generationTimer = 0f;
    private bool simulationIsRunning = false;

    private long k = 0; //update counter

    public static event Action PopulationCreated;


    private void Awake()
    {
        //StartCoroutine(DelayUpdate());
        InitializeEnvironment();
        InitializePopulations();
        PopulationCreated?.Invoke();
    }

    private void Start()
    {
        Debug.Log("Start");
    }

    IEnumerator DelayUpdate()
    {
        Debug.Log("DelayUpdate");
        yield return new WaitForSeconds(100f);
        simulationIsRunning = true;
    }

    //private void Update()
    //{
    //    Debug.Log("Update");
    //    if (!simulationIsRunning)
    //        return;
    //    generationTimer += Time.deltaTime * simulationSpeed;

    //    if (generationTimer >= timeForOneGeneration)
    //    {
    //        generationTimer = 0f;
    //        StepOfGeneration();
    //    }
    //}

    private void FixedUpdate()
    {
        if (k % 5 == 0)  //update only once per 5 physics updates
        {
            k = 0;
            //Debug.Log("FixedUpdate");
            if (!simulationIsRunning)
                return;
            generationTimer += Time.deltaTime * simulationSpeed;

            if (generationTimer >= timeForOneGeneration)
            {
                generationTimer = 0f;
                StepOfGeneration();
            }
        }
        k++;
    }

    #region Initialization

    void InitializeEnvironment()
    {
        env.Initialize();
        env.currentGenerationInEnv = 0;
    }

    //void InitializePopulations()
    //{
    //    foreach (var p in predators)
    //        Destroy(p.gameObject);
    //    foreach (var h in herbivores)
    //        Destroy(h.gameObject);
    //    predators.Clear();
    //    herbivores.Clear();

    //    for (int i = 0; i < predatorCount;  i++)
    //    {
    //        var createPredatorObject = Instantiate(predatorPrefab);
    //        var predatorObject = createPredatorObject.GetComponent<Predator>();
    //        if (predatorObject == null) predatorObject.AddComponent<Predator>();
    //        CreateGenes(predatorObject);
    //        predators.Add(predatorObject);
    //    }
    //    Debug.Log("Predators are ready");

    //    for (int i = 0; i < herbivoreCount; i++)
    //    {
    //        var createHerbivoreObject = Instantiate(herbivorePrefab);
    //        var herbivoreObject = createHerbivoreObject.GetComponent<Herbivore>();
    //        if (herbivoreObject == null) herbivoreObject.AddComponent<Herbivore>();
    //        CreateGenes(herbivoreObject);
    //        herbivores.Add(herbivoreObject);
    //    }
    //    Debug.Log("Herbivores are ready");
    //}

    void InitializePopulations()
    {
        ClearPopulation(predators);
        ClearPopulation(herbivores);

        predators.Clear();
        herbivores.Clear();

        CreatePopulation(
            predatorCount,
            predatorPrefab,
            predatorsPanel,
            predators
        );

        Debug.Log("Predators are ready");

        CreatePopulation(
            herbivoreCount,
            herbivorePrefab,
            herbivoresPanel,
            herbivores
        );

        Debug.Log("Herbivores are ready");
    }

    void CreatePopulation<T>(int count, GameObject prefab, Transform panel, List<T> population) where T : Animal
    {
        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab, panel);

            T animal = instance.GetComponent<T>();
            if (animal == null)
                animal = instance.AddComponent<T>();

            CreateGenes(animal);
            population.Add(animal);
        }
    }

    void ClearPopulation<T>(List<T> population) where T : MonoBehaviour
    {
        foreach (var entity in population)
        {
            if (entity != null)
                Destroy(entity.gameObject);
        }
    }


    void CreateGenes(Animal animal)
    {
        animal.hp = 100f;
        animal.stamina = UnityEngine.Random.Range(staminaRange.min, staminaRange.max);
        animal.speed = UnityEngine.Random.Range(speedRange.min, speedRange.max);
        animal.tempResist = UnityEngine.Random.Range(tempResistRange.min, tempResistRange.max);
        animal.wetResist = UnityEngine.Random.Range(wetResistRange.min, wetResistRange.max);
        animal.eatNeed = UnityEngine.Random.Range(eatNeedRange.min, eatNeedRange.max);
    }

    #endregion


    #region Step of generation

    public void StepOfGeneration()
    {
        // need to count fitnesses for animals
        EvaluateFitnessPredator();
        EvaluateFitnessHerbivore();

        // evolution of animals
        EvolvePredator();
        EvolveHerbivore();

        // (de)buffing the stamina every staminaUpdateInterval
        if (env.currentGenerationInEnv % staminaUpdateInterval == 0)
        {
            ChangeStamina(predators);
            ChangeStamina(herbivores);
        }

        // writing genes to new population
        ApplyNextGeneration(predators, genesNextGenPredators);
        ApplyNextGeneration(herbivores, genesNextGenHerbivores);

        env.NextGeneration();
    }

    #endregion


    #region GA algorithm

    void EvolvePredator()
    {
        genesNextGenPredators = EvolveAnimals(predators);
    }

    void EvolveHerbivore()
    {
        genesNextGenHerbivores = EvolveAnimals(herbivores);
    }

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
            if (UnityEngine.Random.value < crossoverRate)
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
        List<float> geneList = new List<float> { animal.stamina, animal.speed, animal.tempResist, animal.wetResist, animal.eatNeed};
        return geneList;
    }

    Animal TournamentSelection<T>(List<T> list) where T : Animal
    {
        Animal bestAnimal = null;
        // may be can write this method another way
        for (int i = 0; i < tournamentSize; i++) 
        {
            var c = list[UnityEngine.Random.Range(0, list.Count)];
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
            //float envFit = EnvFitness(p);
            float huntFit = predator.huntAbility;
            predator.score = 0.5f + huntFit * 0.5f;
        }
    }

    void EvaluateFitnessHerbivore()
    {
        foreach(var herbivore in herbivores)
        {
            // must count, what important for herbivores?
        }
    }

    void EvaluateFitnessInEnvironment(Animal animal) // ---------------?
    {
        float tempFit;
        float wetFit;
    }

    void CalculateHPPredator()
    {
        
        foreach(var predator in predators)
        {
            predator.hp = 0;
        }
    }

    #endregion


    #region Crossover algorithm

    void CrossoverAlgorithm(List<float> mother, List<float> father)
    {
        // VARIANT 1: single point crossover
        int point = UnityEngine.Random.Range(1, mother.Count);
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
            if (UnityEngine.Random.value < mutationRate)
            {
                // VARIANT 1: without checking allowed interval
                // gene[i] += UnityEngine.Random.Range(-noiseOfMutation, noiseOfMutation); 

                // VARIANT 2: with checking allowed interval
                float noise = UnityEngine.Random.Range(-noiseOfMutation, noiseOfMutation);
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
                // float delta = UnityEngine.Random.Range(-envFactor, envFactor);
            }
        }
    }

    #endregion


    #region Stamina
    // or plus 5?
    public void ChangeStamina<T>(List<T> list) where T : Animal
    {
        foreach (var animal in list)
            animal.stamina = Mathf.Clamp(animal.stamina + UnityEngine.Random.Range(-1f, 1f), staminaRange.min, staminaRange.max);   // should allow to minus the stamina ??
    }

    #endregion


    #region Helpers
    // struct for applying intervals of characteristics
    // [System.Serializable]
    public struct TraitRange        
    {
        public float min, max;
        public TraitRange(float min, float max) 
        { 
            this.min = min; 
            this.max = max; 
        }
    }

    // method for rewriting genes in new population
    void ApplyNextGeneration<T>(List<T> animals, List<List<float>> genes) where T : Animal
    {
        for (int i = 0; i < animals.Count; i++)
        {
            animals[i].stamina = genes[i][0];
            animals[i].speed = genes[i][1];
            animals[i].tempResist = genes[i][2];
            animals[i].wetResist = genes[i][3];
            animals[i].eatNeed = genes[i][4];
            animals[i].score = 0f;

            animals[i].UpdateUI();
        }
    }

    // reading value from slider in UI
    public void SetSimulationSpeed(float speed)
    {
        simulationSpeed = speed;
    }
    #endregion


    #region Actions

    private void OnEnable()
    {
        Environment.OnEnvironmentChanged += OnEnvironmentChanged;
    }

    private void OnDisable()
    {
        Environment.OnEnvironmentChanged -= OnEnvironmentChanged;
    }

    // MAY BE DOESNT NEED IT - DESTROYING OLD LIST OF ANIMALS
    void OnEnvironmentChanged()
    {
    //    if (env.currentGenerationInEnv == 0)
    //        InitializePopulations();
    }

    #endregion
}