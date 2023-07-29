using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Part : MonoBehaviour
{
    private readonly List<Part> subParts = new();

    public void AddPart(Part part) => subParts.Add(part);
}