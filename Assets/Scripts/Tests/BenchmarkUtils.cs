using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BenchmarkUtils
{
    public static void BenchmarkAnimalBatch<T>(
        List<T> animals,
        List<Metric<T>> metrics,
        Environment env,
        Action<Environment, int, List<float[]>> logAction
        ) where T : Animal
    {
        int count = animals.Count;

        foreach(var metric in metrics)
        {
            if (metric.Buffer == null || metric.Buffer.Length != count)
                metric.Buffer = new float[count];
        } 
        
        for (int i = 0;  i < count; i++)
        {
            var animal = animals[i];

            foreach (var metric in metrics)
                metric.Buffer[i] = metric.Selector(animal);
        }

        // collect all the buffers for using in future in Benchmark file
        var buffers = metrics.Select(m => m.Buffer).ToList();

        logAction(env, env.currentGenerationInEnv, buffers);
    }

}
