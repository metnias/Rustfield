using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Robot : Part
{
    private readonly List<PLocomotive> subLocomotives = new();
}