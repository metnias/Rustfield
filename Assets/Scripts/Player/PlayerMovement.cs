using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rBody;

    [SerializeField, Range(0f, 100f)]
    private float velocity = 1f;
    [SerializeField, Range(0f, 100f)]
    private float acceleration = 2f;

    private Vector2 move = Vector2.zero;


    private void FixedUpdate()
    {
        Vector3 targetVel = new(move.x * velocity, rBody.velocity.y, move.y * velocity);
        targetVel = Vector3.MoveTowards(rBody.velocity, targetVel, Time.fixedDeltaTime * acceleration);
        rBody.velocity = targetVel;
    }

#region InputSystem
#pragma warning disable IDE0051
    private void OnMovement(InputValue value)
    {
        move = value.Get<Vector2>();
    }

#pragma warning restore IDE0051
#endregion InputSystem

}
