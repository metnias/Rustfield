using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rBody;

    [SerializeField, Range(0f, 100f)]
    private float speed = 1f;
    [SerializeField, Range(0f, 100f)]
    private float acceleration = 2f, airAcceleration = 1f;
    [SerializeField, Range(0f, 100f)]
    private float jumpStrength = 10f;
    [SerializeField, Range(0f, 90f)]
    private float maxGroundAngle = 60f;
    [SerializeField, Range(0f, 20f)]
    private float gravity = 1f;

    private Vector2 move = Vector2.zero;
    private float wantToJump = 0f;
    private bool onGround = false;
    private float minGroundDotProduct;
    private Vector3 contactNormal = Vector3.up;
    private int stepsSinceLastGrounded = 0;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Start()
    {
        OnValidate();
    }

    private void FixedUpdate()
    {
        Vector3 velocity = rBody.velocity;
        UpdateState();

        velocity += gravity * Time.fixedDeltaTime * Physics.gravity;
        if (wantToJump > 0f)
        {
            wantToJump -= Time.fixedDeltaTime;
            if (onGround)
            {
                wantToJump = 0f;
                Jump();
            }
        }
        AdjustVelocity();
        rBody.velocity = velocity;
        ClearState();

        void UpdateState()
        {
            if (onGround)
            {
                stepsSinceLastGrounded = 0;
                contactNormal.Normalize();
            }
            else
            {
                ++stepsSinceLastGrounded;
                contactNormal = Vector3.up;
            }
        }
        void Jump()
        {
            float jumpSpeed = jumpStrength;
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            velocity += contactNormal * jumpSpeed;
        }
        void AdjustVelocity()
        {
            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);
            float acc = onGround ? acceleration : airAcceleration;
            float maxSpeedChange = acc * Time.fixedDeltaTime;

            float newX = Mathf.MoveTowards(currentX, speed * move.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, speed * move.y, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }
        Vector3 ProjectOnContactPlane(Vector3 vector) => vector - contactNormal * Vector3.Dot(vector, contactNormal);
        void ClearState()
        {
            onGround = false;
            contactNormal = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                onGround = true;
                contactNormal += normal;
            }
        }
    }

    #region InputSystem
#pragma warning disable IDE0051
    private void OnMovement(InputValue value)
    {
        move = value.Get<Vector2>();
        move = Vector2.ClampMagnitude(move, 1f);
    }

    private void OnJump(InputValue _)
    {
        wantToJump = 0.5f;
    }

#pragma warning restore IDE0051
#endregion InputSystem

}
