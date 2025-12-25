using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneticAlgorithmBrain : MonoBehaviour  // #todo: its better to make GA script (GeneticAlgorithm1.cs) without simulating and just use methods from it
{
    public enum AnimalSpecies
    {
        Predator,
        Herbivore
    }

    [Header("Animal type")]
    public AnimalSpecies species;

    [Header("References")]
    public Environment env;
    [SerializeField] private TMP_Text infoText;

    [Header("Population")]
    public int populationSize = 25;

    [Header("GA parameters")]
    [Range(0f, 1f)] public float mutationRate = 0.05f;
    [Range(0f, 1f)] public float crossoverRate = 0.9f;
    public float noiseOfMutation = 0.1f;
    public int firstDeltaHPLose = 10;
    public int secondDeltaHPLose = 20;
    public int elitismCount = 2;
    public int tournamentSize = 3;
    public int staminaUpdateInterval = 5;
    [Range(0f, 1f)] public float blxAlpha = 0.5f;

    [Header("Fitness weights")]
    public float hpWeight = 4f;
    public float abilitiesWeight = 1f;
    public float eatWeight = 0.5f;
    public float fatSaveConst = 0f;

    [Header("Initial ranges")]
    public TraitRange staminaRange = new(5f, 20f);
    public TraitRange speedRange = new(0.5f, 5f);
    public TraitRange tempResistRange = new(-30f, 50f);
    public TraitRange wetResistRange = new(20f, 100f);
    public TraitRange eatNeedRange = new(1.2f, 2.8f);
    public TraitRange fatSaveRange = new(0f, 1f);

    private List<Animal> population = new();
    private List<List<float>> genesNextGen = new();

    public float startDelay = 0.5f;

    [Header("Era settings")]
    public float eraDelay = 2f;


    private void Start()
    {
        StartCoroutine(EraCycleCoroutine());
    }

    private IEnumerator EraCycleCoroutine()
    {
        yield return new WaitUntil(() => env.IsInitialized);

        while (true)
        {
            // init of new era
            if (env.currentGenerationInEnv == 0)
            {
                Debug.Log("Initializing population for new era...");
                InitializePopulation();
            }

            // GA for current era
            yield return StartCoroutine(RunGA());

            // change env to new era
            env.StartNewEra();
            Debug.Log("Era changed!");

            // game doesnt stop, but wait for deltatime
            float timer = 0f;
            while (timer < eraDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator RunGA()
    {
        InitializePopulation();

        for (int generation = 0; generation < env.generationMax; generation++)
        {
            CalculateHP();
            EvaluateFitness();

            genesNextGen = EvolveAnimals(population);
            ApplyNextGeneration(population, genesNextGen);

            if (generation % staminaUpdateInterval == 0)
                ChangeStamina(population);

            env.NextGeneration();

            Debug.Log($"Generation {generation} complete");

            yield return null;
        }

        Animal best = FindBestAnimal(population);
        ApplyBestToThisAnimal(best);

        yield return null;
    }

    #region GA methods

    void CalculateHP()
    {
        if (species == AnimalSpecies.Predator)
            CalculateHPPredator();
        else
            CalculateHPHerbivore();
    }

    void CalculateHPPredator()
    {
        foreach (var animal in population)
        {
            float temp = 0;
            temp = CalculateDelta(animal.tempResist, env.temp) + CalculateDelta(animal.wetResist, env.wet);
            animal.hp -= temp;
        }
    }
    void CalculateHPHerbivore()
    {
        foreach (var animal in population)
        {
            float temp = 0;
            temp = CalculateDelta(animal.tempResist, env.temp) + CalculateDelta(animal.wetResist, env.wet);
            animal.hp -= temp;
        }
    }

    void ApplyNextGeneration<T>(List<T> animals, List<List<float>> genes) where T : Animal
    {
        for (int i = 0; i < animals.Count; i++)
        {
            animals[i].stamina = genes[i][0];
            animals[i].speed = genes[i][1];
            animals[i].tempResist = genes[i][2];
            animals[i].wetResist = genes[i][3];
            animals[i].eatNeed = genes[i][4];
            animals[i].fatSave = genes[i][5];
            animals[i].score = 0f;
        }
    }

    void InitializePopulation()
    {
        population.Clear();

        for (int i = 0; i < populationSize; i++)
        {
            Animal a = new Animal();
            CreateGenes(a);
            population.Add(a);
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

    void EvaluateFitness()
    {
        if (species == AnimalSpecies.Predator)
            EvaluateFitnessPredator();
        else
            EvaluateFitnessHerbivore();
    }

    void EvaluateFitnessPredator()
    {
        foreach (var predator in population)
        {
            // must count, what important for predators?

            // normilizing
            float hpLose = Math.Abs(100 - predator.hp);
            float speedN = Mathf.Clamp01(predator.speed / speedRange.max);
            float staminaN = Mathf.Clamp01(predator.stamina / staminaRange.max);
            float hpLoseN = Mathf.Clamp01(hpLose / 100);
            float eatN = 0;
            if (env.difficultyMode == Environment.DifficultyMode.easyMode)
                eatN = (population.Count * predator.eatNeed) / 70;
            if (env.difficultyMode == Environment.DifficultyMode.mediumMode)
                eatN = (population.Count * predator.eatNeed) / 60;
            if (env.difficultyMode == Environment.DifficultyMode.hardMode)
                eatN = (population.Count * predator.eatNeed) / 40;

            float abilitiesBonus = (speedN + staminaN) * 0.5f;
            float hpPenalty = Mathf.Exp(-hpLose * hpWeight);
            float eatPenalty = 1f - eatN;

            predator.score = 100 * (hpPenalty * (abilitiesBonus * abilitiesWeight + eatPenalty * eatWeight));

            // Debug.Log($"PREDATOR - SCORE: {predator.score} ||| speedN: {speedN} | staminaN: {staminaN} | hpLoseN: {hpLoseN} | eatN: {eatN} ||| abilitiesBonus: {abilitiesBonus} | hpPenalty: {hpPenalty} | eatPenalty: {eatPenalty}");
        }
    }

    void EvaluateFitnessHerbivore()
    {
        foreach (var herbivore in population)
        {
            // must count, what important for herbivores?

            // normilizing
            float hpLose = Math.Abs(100 - herbivore.hp);
            float speedN = Mathf.Clamp01(herbivore.speed / speedRange.max);
            float staminaN = Mathf.Clamp01(herbivore.stamina / staminaRange.max);
            float hpLoseN = Mathf.Clamp01(hpLose / 100);
            float eatN = 0;
            if (env.difficultyMode == Environment.DifficultyMode.easyMode)
                eatN = (population.Count * herbivore.eatNeed) / 70;
            if (env.difficultyMode == Environment.DifficultyMode.mediumMode)
                eatN = (population.Count * herbivore.eatNeed) / 60;
            if (env.difficultyMode == Environment.DifficultyMode.hardMode)
                eatN = (population.Count * herbivore.eatNeed) / 50;

            float abilitiesBonus = (speedN + staminaN) * 0.5f;
            float fatSaveBonus = 0;
            if (herbivore.fatSave >= 0.5f)
                fatSaveBonus = 1;
            float hpPenalty = Mathf.Exp(-hpLose * hpWeight);
            float eatPenalty = 1f - eatN;

            herbivore.score = 100 * (hpPenalty * (abilitiesBonus * abilitiesWeight + eatPenalty * eatWeight + fatSaveConst * fatSaveBonus));

            // Debug.Log($"HERBIVORE - SCORE: {herbivore.score} ||| speedN: {speedN} | staminaN: {staminaN} | hpLoseN: {hpLoseN} | eatN: {eatN} ||| abilitiesBonus: {abilitiesBonus} | hpPenalty: {hpPenalty} | eatPenalty: {eatPenalty}");
        }
    }

    public float CalculateDelta(float animalCharact, float envCharact)
    {
        float penalty = 0;
        float delta = Math.Abs(animalCharact - envCharact);
        float deviation = (100 * delta) / envCharact;
        if (deviation <= firstDeltaHPLose)
        {
            penalty = 0;
        }
        else if (deviation > firstDeltaHPLose && deviation <= secondDeltaHPLose)
        {
            penalty = 1;
        }
        else if (deviation > secondDeltaHPLose)
        {
            penalty = 3;
        }
        return penalty;
    }

    List<List<float>> EvolveAnimals(List<Animal> animals)
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
            // selecting parents - Fitness-Proportionate-Selection
            Animal mother = FitnessProportionateSelection(animals);
            Animal father = FitnessProportionateSelection(animals);

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

    Animal FitnessProportionateSelection(List<Animal> list)
    {
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

        return list[list.Count - 1];
    }

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
                ClampGene(i, gene[i]);

                // VARIANT 3: mutaion adapting to difficultyMode of the env
                // float envFactor = env.difficultyMode == Environment.DifficultyMode.hardMode ? 0.2f : 0.05f;
                // float delta = UnityEngine.Random.Range(-envFactor, envFactor);
            }
        }
    }

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

    void ChangeStamina(List<Animal> list) 
    {
        foreach (var animal in list)
            animal.stamina = Mathf.Clamp(animal.stamina + UnityEngine.Random.Range(-1f, 1f), staminaRange.min, staminaRange.max);   // should allow to minus the stamina ??
    }

    Animal FindBestAnimal(List<Animal> animals)
    {
        Animal best = animals[0];
        foreach (var animal in animals)
        {
            if (animal.score > best.score)
                best = animal;
        }
        return best;
    }

    void ApplyBestToThisAnimal(Animal best)
    {
        Animal target = GetComponent<Animal>();

        target.hp = best.hp;
        target.stamina = best.stamina;
        target.speed = best.speed;
        target.tempResist = best.tempResist;
        target.wetResist = best.wetResist;
        target.eatNeed = best.eatNeed;
        target.fatSave = best.fatSave;

        infoText.text =
            $"BEST {species}\n" +
            $"HP: {target.hp}\n" +
            $"Stamina: {target.stamina:F2}\n" +
            $"Speed: {target.speed:F2}\n" +
            $"Temp: {target.tempResist:F1}\n" +
            $"Wet: {target.wetResist:F1}\n" +
            $"Fat: {target.fatSave:F1}";
    }

    public struct TraitRange
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
