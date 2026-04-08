using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PythonScriptManager : MonoBehaviour
{
    [SerializeField] GeneticAlgorithm1 genAlg;
    public PythonScriptManager(GeneticAlgorithm1 genAlg)
    {
        this.genAlg = genAlg;
    }
    public void StartPyScript(string name, string type)
    {
        // todo: add check of existng file
        // and mb list of names of files
        // and mb this method will contain a lot of methods or recieve a lot of names {name} and after that just opening several plots .py
        // and rewrite loggers for new system of reading plots !!!
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"C:\Users\Viktoria\AppData\Local\Programs\Python\Python314\python.exe", // or "python"
            Arguments = @$"D:\ITMO_diploma\evolutionary_algorithm_agents_in_environment\Assets\Scripts\Tests\py\plot_{name}_{type}.py",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false
        };

        Process.Start(psi);
    }
}
