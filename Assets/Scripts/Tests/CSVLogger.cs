using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditorInternal.VR;
using UnityEngine;

public class CSVLogger
{
    private string predatorPath;
    private string herbivorePath;

    public CSVLogger()
    {
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

    public void WritePredators(List<Predator> predators, Environment env, List<AnimalLogData> logData)
    {
        WriteAgents(predatorPath, predators, env, logData);
    }

    public void WriteHerbivores(List<Herbivore> herbivores, Environment env, List<AnimalLogData> logData)
    {
        WriteAgents(herbivorePath, herbivores, env, logData);
    }

    void WriteAgents<T>(string path, List<T> animals, Environment env, List<AnimalLogData> logData) where T : Animal
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < animals.Count; i++)
        {
            var a = animals[i];
            var d = logData[i];

            sb.AppendLine(string.Join(";",
                env.era,
                env.currentGenerationInEnv,
                env.difficultyMode,
                env.temp,
                env.wet,
                env.windStrength,
                env.eatPredator,
                env.eatHerbivore,
                i,
                a.score,
                a.hp,
                a.stamina,
                a.speed,
                a.tempResist,
                a.wetResist,
                a.fatSave,
                a.eatNeed,
                d.speedN,
                d.staminaN,
                d.hpLoseN,
                d.eatN,
                d.abilitiesBonus,
                d.hpPenalty,
                d.eatPenalty
            ));
        }

        File.AppendAllText(path, sb.ToString());
    }
}