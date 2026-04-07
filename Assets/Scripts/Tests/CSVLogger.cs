using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditorInternal.VR;
using UnityEngine;
using static Environment;

public class CSVLogger
{
    private string predatorPath;
    private string herbivorePath;

    [SerializeField] GeneticAlgorithm1 genAlg;

    public CSVLogger(GeneticAlgorithm1 genAlg)
    {
        this.genAlg = genAlg;

        predatorPath = Path.Combine(Application.dataPath, "predators.csv");
        herbivorePath = Path.Combine(Application.dataPath, "herbivores.csv");

        PrepareFile(predatorPath);
        PrepareFile(herbivorePath);
    }
    void PrepareFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        WriteHeader(path);
    }

    void WriteHeader(string path)
    {
        if (File.Exists(path)) return;

        string header =
            "era;generation;difficultyMode;temp;wet;windStrength;eatPredator;eatHerbivore;" +
            "agentIndex;score;hp;stamina;speed;tempResist;wetResist;fatSave;eatNeed;" +
            "speedN;staminaN;hpLoseN;eatN;abilitiesBonus;hpPenalty;eatPenalty";

        File.WriteAllText(path, header + "\n");
    }

    public void WritePredators(List<Predator> predators, Environment env)
    {
        WriteAgents(predatorPath, predators, env);
    }

    public void WriteHerbivores(List<Herbivore> herbivores, Environment env)
    {
        WriteAgents(herbivorePath, herbivores, env);
    }

    void WriteAgents<T>(string path, List<T> animals, Environment env) where T : Animal
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < animals.Count; i++)
        {
            var a = animals[i];

            // todo: it was a fast way - change in future
            float hpLose = Mathf.Abs(100 - a.hp);
            float speedN = Mathf.Clamp01(a.speed / genAlg.speedRange.max);
            float staminaN = Mathf.Clamp01(a.stamina / genAlg.staminaRange.max);
            float hpLoseN = Mathf.Clamp01(hpLose / 100f);
            float hpPenalty = 0f;
            float abilitiesBonus = 0f;
            float eatN = 0f;
            float deltaEat = 0f;
            float eatPenalty = 0f;

            deltaEat = env.eatPredator - animals[i].eatNeed;

            if (deltaEat < 0)
            {
                if (env.difficultyMode == DifficultyMode.easyMode)
                    eatN = Mathf.Clamp01(animals[i].eatNeed / env.eatPred_Range_Easy.max);
                else if (env.difficultyMode == DifficultyMode.mediumMode)
                    eatN = Mathf.Clamp01(animals[i].eatNeed / env.eatPred_Range_Medium.max);
                else if (env.difficultyMode == DifficultyMode.hardMode)
                    eatN = Mathf.Clamp01(animals[i].eatNeed / env.eatPred_Range_Hard.max);
                eatPenalty = Mathf.Exp(eatN * genAlg.eatWeight);
            }

            abilitiesBonus = Mathf.Exp((speedN + staminaN) * genAlg.abilitiesWeight);
            hpPenalty = Mathf.Exp(hpLoseN * genAlg.hpWeight);

            sb.AppendLine(string.Join(";",
                env.era.ToString(),
                env.currentGenerationInEnv.ToString(),
                env.difficultyMode.ToString(),
                env.temp.ToString(),
                env.wet.ToString(),
                env.windStrength.ToString(),
                env.eatPredator.ToString(),
                env.eatHerbivore.ToString(),
                i.ToString(),
                a.score.ToString(),
                a.hp.ToString(),
                a.stamina.ToString(),
                a.speed.ToString(),
                a.tempResist.ToString(),
                a.wetResist.ToString(),
                a.fatSave.ToString(),
                a.eatNeed.ToString(),
                speedN.ToString(),
                staminaN.ToString(),
                hpLoseN.ToString(),
                eatN.ToString(),
                abilitiesBonus.ToString(),
                hpPenalty.ToString(),
                eatPenalty.ToString()
            ));
        }

        File.AppendAllText(path, sb.ToString());
    }
}