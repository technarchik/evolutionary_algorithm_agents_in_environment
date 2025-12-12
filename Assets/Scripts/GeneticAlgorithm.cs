using System.Collections.Generic;
using UnityEngine;

// Костяк генетического алгоритма для описанной задачи
// Поддерживает: инициализацию среды, генерацию популяции (25 хищников + 25 травоядных),
// фитнесс-функцию, селекцию (турнир), кроссовер (одноточечный), мутацию,
// элитизм, управление stamina (обновляется каждые N поколений),
// каждая инициализация мира длится generationsPerEnvironment поколений.

public class GeneticAlgorithm : MonoBehaviour
{
    [Header("References")]
    public Environment env; // ссылка на Environment в сцене
    public GameObject predatorPrefab;
    public GameObject herbivorePrefab;

    [Header("Population")]
    public int populationSize = 50;
    public int predatorsCount = 25;
    public int herbivoresCount = 25;

    [Header("Environment / Generations")]
    public int generationsPerEnvironment = 50; // сколько поколений держится одна инициализация среды
    public int currentGenerationInEnv = 0;

    [Header("Stamina settings")]
    public int staminaUpdateInterval = 5; // stamina может меняться только каждые N поколений

    [Header("GA parameters")]
    [Range(0f, 1f)] public float mutationRate = 0.05f;
    [Range(0f, 1f)] public float crossoverRate = 0.9f;
    public int elitismCount = 2; // сколько лучших переносим в след. поколение
    public int tournamentSize = 3;

    [Header("Initial ranges (inclusive)")]
    public TraitRange staminaRange = new TraitRange(5f, 20f);
    public TraitRange speedRange = new TraitRange(0.5f, 5f);
    public TraitRange tempResistRange = new TraitRange(0f, 1f);
    public TraitRange wetResistRange = new TraitRange(0f, 1f);
    public TraitRange eatNeedRange = new TraitRange(0.5f, 2f);

    [Header("Runtime")]
    // Списки агентов текущей популяции
    private List<Animal> population = new List<Animal>();

    // Временное хранилище для нового поколения
    private List<List<float>> nextGenerationGenes = new List<List<float>>();

    // Маппинг порядка генов: stamina, speed, temp_resist, wet_resist, eat_need

    private void Start()
    {
        if (env == null) Debug.LogWarning("Environment not assigned to GeneticAlgorithm");

        InitializeEnvironmentAndPopulation(true);
    }

    #region Initialization
    public void InitializeEnvironmentAndPopulation(bool randomizeFirstGeneration)
    {
        // Инициализируем среду (на старте генерируем вручную заранее в Env или вызываем GenerateConditions)
        env.GenerateConditions();
        env.temp = env.TempCalc(env.temp, env.wet, env.wind_strength);
        currentGenerationInEnv = 0;

        // Очищаем старую популяцию (если есть)
        foreach (var a in population)
        {
            if (a != null) Destroy(a.gameObject);
        }
        population.Clear();

        // Создаём популяцию: первые поколение либо рандомное, либо можете загрузить из сохранённых хромосом
        for (int i = 0; i < predatorsCount; i++)
        {
            var go = Instantiate(predatorPrefab);
            var pred = go.GetComponent<Predator>();
            if (pred == null) pred = go.AddComponent<Predator>();
            population.Add(pred);

            if (randomizeFirstGeneration)
                ApplyRandomGenes(pred);
        }
        for (int i = 0; i < herbivoresCount; i++)
        {
            var go = Instantiate(herbivorePrefab);
            var herb = go.GetComponent<Herbivore>();
            if (herb == null) herb = go.AddComponent<Herbivore>();
            population.Add(herb);

            if (randomizeFirstGeneration)
                ApplyRandomGenes(herb);
        }
    }

    private void ApplyRandomGenes(Animal a)
    {
        // hp задаём максимальным и НЕ изменяем
        a.hp = 100f; // можно вынести в публичное поле

        a.stamina = Random.Range(staminaRange.min, staminaRange.max);
        a.speed = Random.Range(speedRange.min, speedRange.max);
        a.temp_resist = Random.Range(tempResistRange.min, tempResistRange.max);
        a.wet_resist = Random.Range(wetResistRange.min, wetResistRange.max);
        a.eat_need = Random.Range(eatNeedRange.min, eatNeedRange.max);

        a.score = 0f;
    }
    #endregion

    #region GA Core Loop (public API)
    // Вызывайте этот метод каждый тик/эпизод (например, после того как провели симуляцию поколения)
    public void OnGenerationFinished()
    {
        // оцениваем фитнесы
        EvaluatePopulationFitness();

        // сохраняем лучшие (опционально: логировать)

        // создаём следующее поколение хромосом
        EvolvePopulation();

        currentGenerationInEnv++;
        if (currentGenerationInEnv >= generationsPerEnvironment)
        {
            // переинициализируем среду
            env.GenerateConditions();
            env.temp = env.TempCalc(env.temp, env.wet, env.wind_strength);
            currentGenerationInEnv = 0;
        }

        // обновление stamina только каждые staminaUpdateInterval поколений в текущей среде
        if (currentGenerationInEnv % staminaUpdateInterval == 0)
        {
            ApplyStaminaChangesIfNeeded();
        }

        // применим nextGenerationGenes к реальным объектам
        ApplyNextGeneration();
    }
    #endregion

    #region Fitness / Evaluation
    private void EvaluatePopulationFitness()
    {
        foreach (var a in population)
        {
            a.score = FitnessFunction(a);
        }
    }

    // Пример фитнес-функции: зависит от соответствия устойчивостей значениям среды,
    // наличия достаточной скорости для взаимодействий и низкой eat_need (меньше потребность — легче выжить)
    // Возвращает float — чем больше, тем лучше
    public float FitnessFunction(Animal a)
    {
        float fit = 0f;

        // Чем ближе temp_resist к текущей temp (нормализуем предположительно в диапазоне [0..1])
        // Для демонстрации делаем простые расчёты — перед использованием в научной работе замените на валидную модель
        float tempFactor = 1f - Mathf.Abs(a.temp_resist - NormalizeTemp(env.temp));
        float wetFactor = 1f - Mathf.Abs(a.wet_resist - NormalizeWet(env.wet));

        // скорость и выносливость дают способность к взаимодействию (охота/побег)
        float mobility = a.speed * (a.stamina / (staminaRange.max + 0.0001f));

        // чем меньше eat_need (при той же еде в среде) — тем лучше
        float foodAdvantage = 1f / (1f + a.eat_need);

        fit = tempFactor * 0.35f + wetFactor * 0.25f + mobility * 0.25f + foodAdvantage * 0.15f;

        // штрафы за отрицательные или NaN значения
        if (float.IsNaN(fit) || float.IsInfinity(fit)) fit = 0f;
        if (fit < 0f) fit = 0f;

        return fit;
    }

    private float NormalizeTemp(float temp)
    {
        // Преобразуем температуру в [0,1] в зависимости от ожидаемого диапазона среды (-30..50)
        return Mathf.InverseLerp(-30f, 50f, temp);
    }
    private float NormalizeWet(float wet)
    {
        // влажность в 0..100
        return Mathf.InverseLerp(0f, 100f, wet);
    }
    #endregion

    #region Selection / Crossover / Mutation / Elitism
    private void EvolvePopulation()
    {
        // сортируем популяцию по фитнесу (убывание)
        population.Sort((a, b) => b.score.CompareTo(a.score));

        nextGenerationGenes.Clear();

        // элитизм: копируем гены лучших
        for (int i = 0; i < Mathf.Min(elitismCount, population.Count); i++)
        {
            nextGenerationGenes.Add(GenesFromAnimal(population[i]));
        }

        // генерируем остальное с помощью селекции, кроссовера и мутации
        while (nextGenerationGenes.Count < population.Count)
        {
            Animal parentA = TournamentSelection();
            Animal parentB = TournamentSelection();

            List<float> childGenesA = new List<float>(GenesFromAnimal(parentA));
            List<float> childGenesB = new List<float>(GenesFromAnimal(parentB));

            // кроссовер
            if (Random.value < crossoverRate)
            {
                SinglePointCrossover(childGenesA, childGenesB);
            }

            // мутация
            MutateGenes(childGenesA);
            MutateGenes(childGenesB);

            nextGenerationGenes.Add(childGenesA);
            if (nextGenerationGenes.Count < population.Count)
                nextGenerationGenes.Add(childGenesB);
        }
    }

    private Animal TournamentSelection()
    {
        Animal best = null;
        for (int i = 0; i < tournamentSize; i++)
        {
            var candidate = population[Random.Range(0, population.Count)];
            if (best == null || candidate.score > best.score) best = candidate;
        }
        return best;
    }

    // Преобразование животного в гены (исключаем hp)
    private List<float> GenesFromAnimal(Animal a)
    {
        return new List<float>
        {
            a.stamina,
            a.speed,
            a.temp_resist,
            a.wet_resist,
            a.eat_need
        };
    }

    private void SinglePointCrossover(List<float> a, List<float> b)
    {
        int len = Mathf.Min(a.Count, b.Count);
        int point = Random.Range(1, len); // точка кроссовера
        for (int i = point; i < len; i++)
        {
            float tmp = a[i]; a[i] = b[i]; b[i] = tmp;
        }
    }

    private void MutateGenes(List<float> genes)
    {
        // простая реализация: для каждого гена с вероятностью mutationRate добавляем небольшой шум
        for (int i = 0; i < genes.Count; i++)
        {
            if (Random.value < mutationRate)
            {
                // масштаб мутации можно сделать пропорциональным диапазону конкретного гена
                switch (i)
                {
                    case 0: // stamina
                        genes[i] += Random.Range(-(staminaRange.max - staminaRange.min) * 0.1f, (staminaRange.max - staminaRange.min) * 0.1f);
                        genes[i] = Mathf.Clamp(genes[i], staminaRange.min, staminaRange.max);
                        break;
                    case 1: // speed
                        genes[i] += Random.Range(-(speedRange.max - speedRange.min) * 0.1f, (speedRange.max - speedRange.min) * 0.1f);
                        genes[i] = Mathf.Clamp(genes[i], speedRange.min, speedRange.max);
                        break;
                    case 2: // temp_resist
                        genes[i] += Random.Range(-0.1f, 0.1f);
                        genes[i] = Mathf.Clamp01(genes[i]);
                        break;
                    case 3: // wet_resist
                        genes[i] += Random.Range(-0.1f, 0.1f);
                        genes[i] = Mathf.Clamp01(genes[i]);
                        break;
                    case 4: // eat_need
                        genes[i] += Random.Range(-(eatNeedRange.max - eatNeedRange.min) * 0.1f, (eatNeedRange.max - eatNeedRange.min) * 0.1f);
                        genes[i] = Mathf.Clamp(genes[i], eatNeedRange.min, eatNeedRange.max);
                        break;
                }
            }
        }
    }
    #endregion

    #region Apply Next Generation
    private void ApplyNextGeneration()
    {
        // Предполагаем, что порядок типов животных сохраняется (первые predatorsCount — хищники и т.д.)
        for (int i = 0; i < population.Count; i++)
        {
            ApplyGenesToAnimal(population[i], nextGenerationGenes[i]);
        }
    }

    private void ApplyGenesToAnimal(Animal a, List<float> genes)
    {
        // genes: stamina, speed, temp_resist, wet_resist, eat_need
        a.stamina = genes[0];
        a.speed = genes[1];
        a.temp_resist = genes[2];
        a.wet_resist = genes[3];
        a.eat_need = genes[4];

        // hp не трогаем — остаётся максимальным
        a.score = 0f;
    }
    #endregion

    #region Stamina updates and utilities
    private void ApplyStaminaChangesIfNeeded()
    {
        // Здесь можно изменить stamina по какому-то правилу (например, уменьшать из-за усталости среды или давать бонус лучшим)
        // По умолчанию — ничего не меняем

        // Пример: дать случайную небольшую корректировку stamina всем агентам
        for (int i = 0; i < population.Count; i++)
        {
            var a = population[i];
            float delta = Random.Range(-1f, 1f);
            a.stamina = Mathf.Clamp(a.stamina + delta, staminaRange.min, staminaRange.max);
        }
    }
    #endregion

    [System.Serializable]
    public struct TraitRange
    {
        public float min;
        public float max;
        public TraitRange(float min, float max)
        {
            this.min = min; this.max = max;
        }
    }

    #region Debug / Helpers
    // Метод для принудительного шага GA (удобно вызывать из инспектора)
    [ContextMenu("Step Generation")]
    public void StepGeneration()
    {
        OnGenerationFinished();
    }
    #endregion
}
