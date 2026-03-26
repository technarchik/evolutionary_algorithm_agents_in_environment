using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using UnityEngine;
using System.Linq;

public class Benchmark_1 : MonoBehaviour
{
    private string filePath;

    private void Start()
    {
        // todo: change name from algorithm for various experiments + structure of folders
        filePath = Path.Combine(Application.dataPath, "results_predator.csv");

        // adding headers
        File.WriteAllText(filePath, "Generation;AvgFitness;MaxFitness\n");
    }

    // call this method after creating every new population
    public void LogGeneration(int generation, float[] fitnessValues)
    {
        if (fitnessValues == null || fitnessValues.Length == 0)
            return;

        float avgFitness = fitnessValues.Average();
        float maxFitness = fitnessValues.Max();

        string line = $"{generation};{avgFitness:F4};{maxFitness:F4}\n";
        File.AppendAllText(filePath, line);
    }
}
