using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RobotAI : PartAI
{
    public RobotAI(Robot owner) : base(owner)
    {
    }

    public Robot OwnerRobot => owner as Robot;
}