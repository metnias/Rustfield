using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Part : MonoBehaviour
{
    protected Part parent = null;

    protected Robot Root => this is Robot root ? root : parent.Root;

    protected readonly List<Part> subParts = new();

    protected readonly List<PWeapon> subWeapons = new();

    protected PartAI AI;

    protected int health = 1;

    #region Builder

    public struct BuildInfo
    {
    }

    public abstract void Build(BuildInfo info);

    public void AddPart(Part part)
    {
        subParts.Add(part);
        if (part is PWeapon pw) subWeapons.Add(pw);
    }

    public void AddAI(PartAI ai) => AI = ai;

    /// <summary>
    /// Call this to root after building is done to calculate part's health
    /// </summary>
    public int CalculateHealth()
    {
        int result = health;
        foreach (var p in subParts) result += p.CalculateHealth();
        health = result;
        return health;
    }

    #endregion Builder
}