using Palmmedia.ReportGenerator.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditorInternal.VR;
using UnityEngine;

public class Benchmark_2 : MonoBehaviour
{
    private string filePath_data;
    private string filePath_best;

    private void Start()
    {
        // todo: change name from algorithm for various experiments + structure of folders
        filePath_data = Path.Combine(Application.dataPath, "results_herbivore.csv");
        // adding headers
        File.WriteAllText(filePath_data, "Generation;AvgFitness;MaxFitness;MinFitness;StdDev\n");

        filePath_best = Path.Combine(Application.dataPath, "results_bests.csv");
        //File.WriteAllText(filePath_best, "Type; Fitness; HP; Stamina; Speed; Temp/TempRes; Wet/WetRes; EatPredator; EatHerbivore; FatSave\n");
    }

    // call this method after creating every new population
    public void LogGeneration(int generation, float[] fitnessValues)
    {
        if (fitnessValues == null || fitnessValues.Length == 0)
            return;

        float avgFitness = fitnessValues.Average();
        float maxFitness = fitnessValues.Max();
        float minFitness = fitnessValues.Min();

        // standard deviation
        float sumSquaredDiffs = 0f;
        for (int i = 0; i < fitnessValues.Length; i++)
        {
            float diff = fitnessValues[i] - avgFitness;
            sumSquaredDiffs += diff * diff;
        }
        float stdDev = Mathf.Sqrt(sumSquaredDiffs / fitnessValues.Length);

        string line = $"{generation};{avgFitness:F4};{maxFitness:F4};{minFitness:F4};{stdDev:F4}\n";
        File.AppendAllText(filePath_data, line);
    }

    public void LogBest(Environment env, Herbivore herbivore)
    {
        //using (StreamWriter writer = new StreamWriter(filePath_best, true)) // true = append
        //{
        //    writer.WriteLine($"{"Best herbivore"};{herbivore.score:F4};{herbivore.hp};{herbivore.stamina:F1};{herbivore.speed:F1};{herbivore.tempResist:F1};{herbivore.wetResist:F1};{"-"};{herbivore.eatNeed};{herbivore.fatSave}");
        //    writer.WriteLine($"{"Environment"};{"-"};{"-"};{"-"};{"-"};{env.temp};{env.wet};{env.eatPredator};{env.eatHerbivore};{"-"}");
        //}
        //string line = $"{"Best herbivore"};{herbivore.score:F4};{herbivore.hp};{herbivore.stamina:F1};{herbivore.speed:F1};{herbivore.tempResist:F1};{herbivore.wetResist:F1};{"-"};{herbivore.eatNeed};{herbivore.fatSave}\n";
        //File.AppendAllText(filePath_best, line);

        //string line1 = $"{"Environment"};{"-"};{"-"};{"-"};{"-"};{env.temp};{env.wet};{env.eatPredator};{env.eatHerbivore};{"-"}";
        //File.AppendAllText(filePath_best, line);
    }
}

//$"BEST {animal.GetType().Name} | " +
//            $"Score: {animal.score:F2} | " +
//            $"HP: {animal.hp} | " +
//            $"Stamina: {animal.stamina:F2}, " +
//            $"Speed: {animal.speed:F2}, " +
//            $"TempRes: {animal.tempResist:F1}, " +
//            $"WetRes: {animal.wetResist:F1}, " +
//            $"EatNeed: {animal.eatNeed:F2} | " +
//            $"FatSave: {animal.fatSave:F1} | " +
//            $"Generation: {env.currentGenerationInEnv}"