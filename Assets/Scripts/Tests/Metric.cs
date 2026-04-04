using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metric<T>
{
    public Func<T, float> Selector;
    public float[] Buffer;
}
