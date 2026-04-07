using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal.VR;
using UnityEngine;
using static Environment;
using static Unity.VisualScripting.LudiqRootObjectEditor;

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
    public bool oneEraMode = false;
    [Range(0f, 1f)] public float mutationProbability = 0.05f;
    [Range(0f, 1f)] public float crossoverProbability = 0.9f;
    public float noiseOfMutation = 0.1f;
    public int firstDeltaHPLose = 10;
    public int secondDeltaHPLose = 20;
    public int elitismCount = 2;
    public int tournamentSize = 3;
    public int staminaUpdateInterval = 5;
    public float crossoverRate = 0.5f;  // trash - used in uniform crossover
    [Range(0f, 1f)] public float blxAlpha = 0.5f;

    [Header("Fitness weights")]
    public float hpWeight = 4f;
    public float abilitiesWeight = 1f;
    public float eatWeight = 0.5f;
    public float fatSaveWeight = 2f;
    public float staminaDebuffing = 2f; // go down because of increasing fatsave ability (heavy animal)

    [Header("Initial ranges")]
    public TraitRange staminaRange = new TraitRange(5f, 20f);       // max was 20f
    public TraitRange speedRange = new TraitRange(0.5f, 5f);
    public TraitRange tempResistRange = new TraitRange(-60f, 100f);
    public TraitRange wetResistRange = new TraitRange(20f, 100f);
    public TraitRange eatNeedRange = new TraitRange(2f, 6f);    // based on logic "everyone is feeded" - for every difficulty mode
    public TraitRange fatSaveRange = new TraitRange(0f, 1f);

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
    private bool simulationIsRunning = true;
    private long k = 0; //update counter

    // for presenting the best animal
    private bool eraPauseActive = false;
    // best animals themself
    Predator bestPredator;
    Herbivore bestHerbivore;

    public static event Action PopulationCreated;

    // benchmarks & loggers
    private CSVLogger csvLogger;
    private PythonScriptManager pythonScriptManager;
    private bool plotAlreadyStarted = false;
    private bool simulationFinished = false;
    [SerializeField] private Benchmark benchmark;
    [SerializeField] private Benchmark benchmarkPredator;
    [SerializeField] private Benchmark benchmarkHerbivore;

    private void Awake()
    {
        // initializing log and benchmark objects
        csvLogger = new CSVLogger(this);
        benchmarkPredator.Init("predator");
        benchmarkHerbivore.Init("herbivore");

        //StartCoroutine(DelayUpdate());
        InitializeEnvironment();
        InitializePopulations();
        PopulationCreated?.Invoke();
    }

    private void Start()
    {
        Debug.Log("Start");
        pythonScriptManager = new PythonScriptManager(this);
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
        if (simulationFinished)
            return;

        if (k % 1 == 0)  // update only once per 1 physics updates
        {
            k = 0;
            
            // Debug.Log("FixedUpdate");
            if (!simulationIsRunning) // for highlighting the best - doesnt work (dont needed anyway)
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
        animal.fatSave = UnityEngine.Random.Range(fatSaveRange.min, fatSaveRange.max);
    }

    #endregion


    #region Step of generation

    public void StepOfGeneration()
    {
        //if (oneEraMode && env.era < 1)
        //{
            Debug.Log($"Зашло в генерацию");
            if (env.currentGenerationInEnv != 0)
            {
                CalculateHPPredator();
                CalculateHPHerbivore();
            }

            // count fitnesses for animals
            EvaluateFitnessPredator();
            EvaluateFitnessHerbivore();

            // logging
            csvLogger.WritePredators(predators, env);
            csvLogger.WriteHerbivores(herbivores, env);

            // BENCHMARKS
            BenchmarkPredator();
            BenchmarkHerbivore();

            // evolution of animals
            EvolvePredator();
            EvolveHerbivore();

            // (de)buffing the stamina every staminaUpdateInterval
            if (env.currentGenerationInEnv % staminaUpdateInterval == 0)
            {
                // ChangeStamina();
                ChangeStamina(predators);
                ChangeStamina(herbivores);
            }

            // for presenting the best animal
            if (env.currentGenerationInEnv + 1 >= env.generationMax && !eraPauseActive)
            {
                bestPredator = FindBestAnimal(predators);
                bestHerbivore = FindBestAnimal(herbivores);

                HighlightBestAnimal(predators, bestPredator, Color.yellow);
                HighlightBestAnimal(herbivores, bestHerbivore, Color.yellow);

                Debug.Log($"--------- END OF ERA {env.era} ---------");
                LogBestAnimal(bestPredator);
                LogBestAnimal(bestHerbivore);

                BenchmarkBestAnimals();

                if (oneEraMode)
                {
                    // ВАЖНО: запускаем Python только один раз
                    if (!plotAlreadyStarted)
                    {
                        plotAlreadyStarted = true;
                        pythonScriptManager.StartPyScript("fitness");
                    }

                    simulationFinished = true;
                    simulationIsRunning = false;
                    return;
                }

                StartCoroutine(WaitAndStartNewEra());
                return;
            }
            //if (env.currentGenerationInEnv + 1 >= env.generationMax && !eraPauseActive)
            //{
            //    // looking for the bests
            //    bestPredator = FindBestAnimal(predators);
            //    bestHerbivore = FindBestAnimal(herbivores);

            //    // highlighting
            //    HighlightBestAnimal(predators, bestPredator, Color.yellow);
            //    HighlightBestAnimal(herbivores, bestHerbivore, Color.yellow);

            //    // log
            //    Debug.Log($"--------- END OF ERA {env.era} ---------");
            //    LogBestAnimal(bestPredator);
            //    LogBestAnimal(bestHerbivore);

            //    // BENCHMARKS
            //    BenchmarkBestAnimals();

            //    StartCoroutine(WaitAndStartNewEra());
            //    return;
            //}

            foreach (var predator in predators)
                predator.UpdateUI();

            foreach (var herbivore in herbivores)
                herbivore.UpdateUI();

            // writing genes to new population
            ApplyNextGeneration(predators, genesNextGenPredators);
            ApplyNextGeneration(herbivores, genesNextGenHerbivores);

            env.NextGeneration();
        //}

        // showing the plots
        //if (env.era == 1 && !plotAlreadyStarted)
        //{
        //    plotAlreadyStarted = true;
        //    pythonScriptManager.StartPyScript("fitness");
        //}
    }

    IEnumerator WaitAndStartNewEra()
    {
        eraPauseActive = true;

        // stop the simulation
        simulationIsRunning = false;

        // pause
        yield return new WaitForSeconds(5f);

        // delete highlighting
        ResetHighlight(predators);
        ResetHighlight(herbivores);

        // return to simulation
        env.NextGeneration();
        simulationIsRunning = true;
        eraPauseActive = false;
    }

    IEnumerator ShowBestAnimal()
    {
        eraPauseActive = true;

        // stop the simulation
        simulationIsRunning = false;

        // looking for the bests
        Predator bestPredator = FindBestAnimal(predators);
        Herbivore bestHerbivore = FindBestAnimal(herbivores);

        // highlighting
        HighlightBestAnimal(predators, Color.yellow);
        HighlightBestAnimal(herbivores, Color.yellow);

        // log
        Debug.Log($"--------- END OF ERA {env.era} ---------");
        LogBestAnimal(bestPredator);
        LogBestAnimal(bestHerbivore);

        // pause
        yield return new WaitForSeconds(5f);

        // delete highlighting
        ResetHighlight(predators);
        ResetHighlight(herbivores);

        // return to simulation
        env.NextGeneration();
        simulationIsRunning = true;
        eraPauseActive = false;
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

        // crossover
        while (children.Count < animals.Count)
        {
            // selecting parents
            // Tournament-Selection
            // Animal mother = TournamentSelection(animals);
            // Animal father = TournamentSelection(animals);
            // Fitness-Proportionate-Selection
            Animal mother = FitnessProportionateSelection(animals);
            Animal father = FitnessProportionateSelection(animals);

            // remember their genes
            var genesMother = TakeGenesFromAnimal(mother);
            var genesFather = TakeGenesFromAnimal(father);

            // crossover mechanism
            if (UnityEngine.Random.value < crossoverProbability)
                CrossoverAlgorithm(genesMother, genesFather);

            // mutation mechanism
            Mutation(genesMother);
            Mutation(genesFather);

            // adding genes to the next generation
            children.Add(genesMother);
            if (children.Count < animals.Count)
                children.Add(genesFather);
        }
        return children;
    }

    List<float> TakeGenesFromAnimal(Animal animal)
    {
        List<float> geneList = new List<float> { animal.stamina, animal.speed, animal.tempResist, animal.wetResist, animal.eatNeed, animal.fatSave };
        return geneList;
    }

    #endregion

    #region Selection methids
    Animal TournamentSelection<T>(List<T> list) where T : Animal
    {
        Animal bestAnimal = null;
        // may be can write this method another way
        for (int i = 0; i < tournamentSize; i++)
        {
            var ind = list[UnityEngine.Random.Range(0, list.Count)];
            if (bestAnimal == null || ind.score > bestAnimal.score)
                bestAnimal = ind;
        }
        return bestAnimal;
    }

    Animal FitnessProportionateSelection<T>(List<T> list) where T : Animal
    {
        // MaxValue cause we need the biggest fitness in the world!
        float minFitness = float.MaxValue;

        foreach (var ind in list)
            minFitness = Mathf.Min(minFitness, ind.score);

        float offset = minFitness < 0 ? -minFitness + 0.001f : 0f;

        float totalFitness = 0f;
        foreach (var ind in list)
            totalFitness += ind.score + offset;

        float randomPoint = UnityEngine.Random.Range(0f, totalFitness);

        float currentSum = 0f;
        foreach (var ind in list)
        {
            currentSum += ind.score + offset;
            if (currentSum >= randomPoint)
                return ind;
        }

        // fallback
        return list[list.Count - 1];
    }

    #endregion


    #region Fitness functions

    void EvaluateFitnessPredator()
    {
        foreach (var predator in predators)
        {
            // must count, what important for predators?

            // normilizing
            float hpLose = Math.Abs(100 - predator.hp);
            float speedN = Mathf.Clamp01(predator.speed / speedRange.max);
            float staminaN = Mathf.Clamp01(predator.stamina / staminaRange.max);
            float hpLoseN = Mathf.Clamp01(hpLose / 100);
            float hpPenalty = 0f;
            float abilitiesBonus = 0f;
            float eatN = 0f;
            float deltaEat = 0f;
            float eatPenalty = 0f;

            deltaEat = env.eatPredator - predator.eatNeed;

            if (deltaEat < 0)
            {
                if (env.difficultyMode == DifficultyMode.easyMode)
                    eatN = Mathf.Clamp01(predator.eatNeed / env.eatPred_Range_Easy.max);
                else if (env.difficultyMode == DifficultyMode.mediumMode)
                    eatN = Mathf.Clamp01(predator.eatNeed / env.eatPred_Range_Medium.max);
                else if (env.difficultyMode == DifficultyMode.hardMode)
                    eatN = Mathf.Clamp01(predator.eatNeed / env.eatPred_Range_Hard.max);
                eatPenalty = Mathf.Exp(eatN * eatWeight);
            }

            abilitiesBonus = Mathf.Exp((speedN + staminaN) * abilitiesWeight);
            hpPenalty = Mathf.Exp(hpLoseN * hpWeight);

            predator.score = (predator.score - hpPenalty + abilitiesBonus - eatPenalty);

            Debug.Log($"PREDATOR - SCORE: {predator.score} ||| speedN: {speedN} | staminaN: {staminaN} | hpLoseN: {hpLoseN} | eatN: {eatN} ||| abilitiesBonus: {abilitiesBonus} | hpPenalty: {hpPenalty} | eatPenalty: {eatPenalty}");
        }
    }

    void EvaluateFitnessHerbivore()
    {
        foreach (var herbivore in herbivores)
        {
            // must count, what important for herbivores?

            // normilizing
            float hpLose = Math.Abs(100 - herbivore.hp);
            float speedN = Mathf.Clamp01(herbivore.speed / speedRange.max);
            float hpLoseN = Mathf.Clamp01(hpLose / 100);
            float hpPenalty = 0f;
            float abilitiesBonus = 0f;
            float fatSaveBonus = 0f;
            float deltaFreeEat = 0f;

            deltaFreeEat = env.eatHerbivore - herbivore.eatNeed;
            float deltaFreeEatN = Mathf.Clamp01(deltaFreeEat);

            if (deltaFreeEatN > 0)
            {
                fatSaveBonus = Mathf.Exp(herbivore.fatSave * (deltaFreeEatN * fatSaveWeight));
            }
            else if (deltaFreeEatN == 0)
            {
                fatSaveBonus = Mathf.Exp(herbivore.fatSave * (fatSaveWeight / 2));
            }

            // debuffing stamina
            if (deltaFreeEatN != 0)
                herbivore.stamina = herbivore.stamina * Math.Abs(deltaFreeEatN);
            // normilizing stamina
            float staminaN = Mathf.Clamp01(herbivore.stamina / staminaRange.max);


            // мб менять staminaN в зависимости от fatsave?
            abilitiesBonus = Mathf.Exp((speedN + staminaN) * abilitiesWeight); // сомнения еще насчет того, стоит ли все вместе считать, может отдельно, как и другие показатели?     #todo

            hpPenalty = Mathf.Exp(hpLoseN * hpWeight);

            herbivore.score = (herbivore.score - hpPenalty + abilitiesBonus + fatSaveBonus);

            Debug.Log($"HERBIVORE - SCORE: {herbivore.score} ||| speedN: {speedN} | staminaN: {staminaN} | hpLoseN: {hpLoseN}  ||| abilitiesBonus: {abilitiesBonus} | hpPenalty: {hpPenalty} | fatSaveBonus: {fatSaveBonus}");
        }
    }

    void CalculateHPPredator() // BTW i dont count hp lose if food is not enough - no, it will be count in stamina deprive
    {
        foreach (var predator in predators)
        {
            float penalty = 0;
            penalty = CalculateDelta(predator.tempResist, env.temp) + CalculateDelta(predator.wetResist, env.wet);
            predator.hp -= penalty;
        }
    }
    void CalculateHPHerbivore() // BTW i dont count hp lose if food is not enough - no, it will be count in stamina deprive
    {
        foreach (var herbivore in herbivores)
        {
            float penalty = 0;
            penalty = CalculateDelta(herbivore.tempResist, env.temp) + CalculateDelta(herbivore.wetResist, env.wet);
            herbivore.hp -= penalty;
        }
    }

    public float CalculateDelta(float animalCharact, float envCharact)
    {
        float penalty = 0;
        float delta = Math.Abs(animalCharact - envCharact);
        penalty = Mathf.Exp(delta * 0.2f);
        //float deviation = (100 * delta) / envCharact;
        //if (deviation <= firstDeltaHPLose)
        //{
        //    penalty = 0;
        //}
        //else if (deviation > firstDeltaHPLose && deviation <= secondDeltaHPLose)
        //{
        //    penalty = 1;
        //}
        //else if (deviation > secondDeltaHPLose)
        //{
        //    penalty = 3;        // #todo мб тут все-таки повыше сделать? 
        //}
        return penalty;
    }

    #endregion


    #region Crossover algorithm

    void CrossoverAlgorithm(List<float> mother, List<float> father)
    {
        //// VARIANT 1: single point crossover
        //int point = UnityEngine.Random.Range(1, mother.Count);
        //for (int i = point; i < mother.Count; i++)
        //{
        //    float temp = mother[i];
        //    mother[i] = father[i];
        //    father[i] = temp;
        //}

        //// VARIANT 2: uniform crossover
        //for (int i = 0; i < mother.Count; i++)
        //{
        //    if (UnityEngine.Random.value < crossoverProbability)
        //    {
        //        float temp = mother[i];
        //        mother[i] = father[i];
        //        father[i] = temp;
        //    }
        //}

        // VARIANT 3: BLX-alpha crossover
        for (int i = 0; i < mother.Count; i++)
        {
            float gene1 = mother[i];
            float gene2 = father[i];

            float min = Mathf.Min(gene1, gene2);
            float max = Mathf.Max(gene1, gene2);
            float range = max - min;

            float left = min - blxAlpha * range;
            float right = max + blxAlpha * range;

            float child1 = UnityEngine.Random.Range(left, right);
            float child2 = UnityEngine.Random.Range(left, right);

            // clamping
            mother[i] = ClampGene(i, child1);
            father[i] = ClampGene(i, child2);
        }
    }

    #endregion


    #region Mutation algorithm

    void Mutation(List<float> gene)
    {
        for (int i = 0; i < gene.Count; i++)
        {
            if (UnityEngine.Random.value < mutationProbability)
            {
                // VARIANT 1: without checking allowed interval
                // gene[i] += UnityEngine.Random.Range(-noiseOfMutation, noiseOfMutation); 

                // VARIANT 2: with checking allowed interval
                float noise = UnityEngine.Random.Range(-noiseOfMutation, noiseOfMutation);
                gene[i] += noise;
                ClampGene(i, gene[i]);

                // VARIANT 3: mutaion adapting to difficultyMode of the env
                // float envFactor = env.difficultyMode == Environment.DifficultyMode.hardMode ? 0.2f : 0.05f;
                // float delta = UnityEngine.Random.Range(-envFactor, envFactor);
            }
        }
    }

    #endregion


    #region Stamina

    // increasing the stamina - this version is trash
    public void ChangeStamina<T>(List<T> list) where T : Animal
    {
        foreach (var animal in list)
            animal.stamina = Mathf.Clamp(animal.stamina + UnityEngine.Random.Range(-1f, 1f), staminaRange.min, staminaRange.max);   // should allow to minus the stamina ??
    }

    // increasing the stamina
    public void ChangeStamina()
    {
        staminaRange.max += 2;
    }
    
    #endregion

    #region Choosing the best

    // searching the best agent
    T FindBestAnimal<T>(List<T> animals) where T : Animal
    {
        T best = animals[0];

        foreach (var animal in animals)
        {
            if (animal.score > best.score)
                best = animal;
        }
        return best;
    }

    // highlighting the best agent - doesnt working :_(    mb cause of using renderer?
    void HighlightBestAnimal<T>(List<T> animals, T bestAnimal, Color highlightColor) where T : Animal
    {
        foreach (var animal in animals)
        {
            if (animal == bestAnimal)
            {
                var renderer = animal.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = highlightColor;
            }
            else
            {
                var renderer = animal.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = Color.white;
            }
        }
    }

    // highlighting the best agent - doesnt working :_(
    void HighlightBestAnimal<T>(List<T> animals, Color highlightColor) where T : Animal
    {
        foreach (var animal in animals)
        {
            var image = animal.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
                continue;

            image.color = Color.white;
        }

        T best = FindBestAnimal(animals);

        var bestImage = best.GetComponent<UnityEngine.UI.Image>();
        if (bestImage != null)
            bestImage.color = highlightColor;

        LogBestAnimal(best);
    }

    void ResetHighlight<T>(List<T> animals) where T : Animal
    {
        foreach (var animal in animals)
        {
            var image = animal.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
                image.color = Color.white;
        }
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
            animals[i].hp = 100f;
            animals[i].stamina = genes[i][0];
            animals[i].speed = genes[i][1];
            animals[i].tempResist = genes[i][2];
            animals[i].wetResist = genes[i][3];
            animals[i].eatNeed = genes[i][4];
            animals[i].fatSave = genes[i][5];
            animals[i].score = 100f;
        }
    }

    // reading value from slider in UI
    public void SetSimulationSpeed(float speed)
    {
        simulationSpeed = speed;
    }

    // clamping genes to the intervals of the characteristics
    float ClampGene(int index, float value)
    {
        switch (index)
        {
            case 0: // stamina
                return Mathf.Clamp(value, staminaRange.min, staminaRange.max);
            case 1: // speed
                return Mathf.Clamp(value, speedRange.min, speedRange.max);
            case 2: // tempResist
                return Mathf.Clamp(value, tempResistRange.min, tempResistRange.max);
            case 3: // wetResist
                return Mathf.Clamp(value, wetResistRange.min, wetResistRange.max);
            case 4: // eatNeed
                return Mathf.Clamp(value, eatNeedRange.min, eatNeedRange.max);
            case 5: // fatSave
                return Mathf.Clamp(value, fatSaveRange.min, fatSaveRange.max);
            default: return value;
        }
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
        if (env.currentGenerationInEnv == 0)
            InitializePopulations();
        // разобраться с тем, как производить эволюцию между эрами - продолжать с тем же набором или все-таки обнулять, как сейчас? #todo
    }

    #endregion

    #region Logs and Benchmarks

    void LogBestAnimal(Animal animal)
    {
        Debug.Log(
            $"BEST {animal.GetType().Name} | " +
            $"Score: {animal.score:F2} | " +
            $"HP: {animal.hp} | " +
            $"Stamina: {animal.stamina:F2}, " +
            $"Speed: {animal.speed:F2}, " +
            $"TempRes: {animal.tempResist:F1}, " +
            $"WetRes: {animal.wetResist:F1}, " +
            $"EatNeed: {animal.eatNeed:F2} | " +
            $"FatSave: {animal.fatSave:F1} | " +
            $"Generation: {env.currentGenerationInEnv}"
        );
    }

    // realization
    List<Metric<Predator>> predatorMetrics = new List<Metric<Predator>>
    {
        new Metric<Predator>{Selector = x => x.score},
        new Metric<Predator>{Selector = x => x.speed},
        new Metric<Predator>{Selector = x => x.stamina},
        new Metric<Predator>{Selector = x => x.eatNeed},
        new Metric<Predator>{Selector = x => x.fatSave},
    };
    List<Metric<Herbivore>> herbivoreMetrics = new List<Metric<Herbivore>>
    {
        new Metric<Herbivore>{Selector = x => x.score},
        new Metric<Herbivore>{Selector = x => x.speed},
        new Metric<Herbivore>{Selector = x => x.stamina},
        new Metric<Herbivore>{Selector = x => x.eatNeed},
        new Metric<Herbivore>{Selector = x => x.fatSave},
    };

    void BenchmarkPredator()
    {
        BenchmarkUtils.BenchmarkAnimalBatch(predators, predatorMetrics, env, (env, gen, buffers) => benchmarkPredator.LogGeneration(gen, buffers, "predator"));
    }

    void BenchmarkHerbivore()
    {
        BenchmarkUtils.BenchmarkAnimalBatch(herbivores, herbivoreMetrics, env, (env, gen, buffers) => benchmarkHerbivore.LogGeneration(gen, buffers, "herbivore"));
    }

    void BenchmarkBestAnimals()
    {
        benchmark.LogEnvInBest(env);
        benchmarkPredator.LogBest(env, bestPredator);
        benchmarkHerbivore.LogBest(env, bestHerbivore);
    }

    #endregion
}