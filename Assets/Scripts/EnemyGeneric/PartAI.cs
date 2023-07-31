using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PartAI
{
    public PartAI(Part owner)
    {
        this.owner = owner;
    }

    public readonly Part owner;

    public virtual void MoveTo()
    {
    }

    public virtual void Attack()
    {
    }
}