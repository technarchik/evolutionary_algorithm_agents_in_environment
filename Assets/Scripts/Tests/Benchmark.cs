using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal.VR;
using UnityEngine;

public class Benchmark : MonoBehaviour
{
    private string filePath_data;
    private string filePath_best;
    private string typeName;

    // creating benchmark files
    public void Init(string type)
    {
        typeName = type;

        filePath_data = Path.Combine(Application.dataPath, $"results_{type}.csv");

        File.WriteAllText(filePath_data,
            "Generation;" +
            "AvgFitness;MaxFitness;MinFitness;StdDevFitness;" +
            "AvgStamina;MaxStamina;MinStamina;StdDevStamina;" +
            "AvgSpeed;MaxSpeed;MinSpeed;StdDevSpeed\n"
        );

        filePath_best = Path.Combine(Application.dataPath, "results_best.csv");
        File.WriteAllText(filePath_best, "Type; Fitness; HP; Stamina; Speed; Temp/TempRes; Wet/WetRes; EatPredator; EatHerbivore; FatSave\n");
    }

    // method for reading arrays of metrics 
    public void LogGeneration(int generation, List<float[]> metricsBuffers, string type)
    {
        string path = Path.Combine(Application.dataPath, $"results_{type}.csv");

        if (metricsBuffers == null || metricsBuffers.Count == 0)
            return;

        // 0 - fitness, 1 - speed, 2 - stamina
        var fitness = metricsBuffers[0];
        var speed = metricsBuffers[1];
        var stamina = metricsBuffers[2];

        string line = generation.ToString();

        line += ComputeStats(fitness);
        line += ComputeStats(speed);
        line += ComputeStats(stamina);

        File.AppendAllText(path, line + "\n");    
    }

    //counting metrics
    private string ComputeStats(float[] values)
    {
        float avg = values.Average();
        float max = values.Max();
        float min = values.Min();

        // standard deviation
        float sum = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float diff = values[i] - avg;
            sum += diff * diff;
        }
        float stdDev = Mathf.Sqrt(sum / values.Length);
        return $";{avg:F4};{max:F4};{min:F4};{stdDev:F4}";
    }

    public void LogBest<T>(Environment env, T best) where T : Animal
    {
        using (StreamWriter writer = new StreamWriter(filePath_best, true))
        {
            if (best is Predator p)
            {
                writer.WriteLine($"{"Best predator"};{p.score:F4};{p.hp};{p.stamina:F1};{p.speed:F1};{p.tempResist:F1};{p.wetResist:F1};{p.eatNeed};{"-"};{p.fatSave}");
            }
            else if (best is Herbivore h)
            {
                writer.WriteLine($"{"Best herbivore"};{h.score:F4};{h.hp};{h.stamina:F1};{h.speed:F1};{h.tempResist:F1};{h.wetResist:F1};{"-"};{h.eatNeed};{h.fatSave}");
            }

        }
    }

    public void LogEnvInBest(Environment env)
    {
        using (StreamWriter writer = new StreamWriter(filePath_best, true))
        {
            writer.WriteLine($"{env.difficultyMode};{"-"};{"-"};{"-"};{"-"};{env.temp};{env.wet};{env.eatPredator};{env.eatHerbivore};{"-"}");
        }
    }
}
