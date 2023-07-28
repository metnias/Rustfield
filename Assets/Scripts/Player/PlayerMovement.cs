using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rBody;

    [SerializeField]
    private Transform playerInputSpace = default;

    [Header("Horizontal")]
    [SerializeField, Range(0f, 100f)]
    private float speed = 1f;

    [SerializeField, Range(0f, 100f)]
    private float acceleration = 2f, airAcceleration = 1f;

    [Header("Vertical")]
    [SerializeField, Range(0f, 100f)]
    private float jumpStrength = 10f;

    [SerializeField, Range(0f, 90f)]
    private float maxGroundAngle = 60f;

    [SerializeField, Range(0f, 20f)]
    private float gravity = 1f;

    [SerializeField]
    private LayerMask standableMask = -1;

    private Vector2 move = Vector2.zero;
    private float wantToJump = 0f;
    private bool OnGround => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;
    private float minGroundDotProduct;
    private Vector3 contactNormal = Vector3.up, steepNormal = Vector3.zero;
    private int groundContactCount = 0, steepContactCount = 0;
    private int stepsSinceLastGrounded = 0, stepsSinceLastJump = 0;

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
            Jump();
        }
        AdjustVelocity();
        rBody.velocity = velocity;
        ClearState();

        void UpdateState()
        {
            if (OnGround || SnapToGround() || CheckSteepContacts())
            {
                stepsSinceLastGrounded = 0;
                contactNormal.Normalize();
            }
            else
            {
                ++stepsSinceLastGrounded;
                contactNormal = Vector3.up;
            }
            ++stepsSinceLastJump;
        }
        bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) return false;
            float curSpeed = velocity.magnitude;
            if (curSpeed > speed * 0.8f) return false;
            if (!Physics.Raycast(rBody.position, Vector3.down, out RaycastHit hit, 0.5f, standableMask)) return false;
            if (hit.normal.y < minGroundDotProduct) return false;
            contactNormal = hit.normal;
            float newSpeed = velocity.magnitude;
            float dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0f) velocity = (velocity - hit.normal * dot).normalized * newSpeed;
            return true;
        }
        bool CheckSteepContacts()
        {
            if (steepContactCount > 1)
            {
                steepNormal.Normalize();
                if (steepNormal.y >= minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                    return true;
                }
            }
            return false;
        }
        void Jump()
        {
            Vector3 jumpDir;
            if (OnGround) jumpDir = contactNormal;
            else if (OnSteep) jumpDir = steepNormal;
            else return;
            jumpDir = (jumpDir + Vector3.up).normalized;

            stepsSinceLastJump = 0; wantToJump = 0f;
            float jumpSpeed = jumpStrength;
            float alignedSpeed = Vector3.Dot(velocity, jumpDir);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            velocity += jumpDir * jumpSpeed;
        }
        void AdjustVelocity()
        {
            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);
            float acc = OnGround ? acceleration : airAcceleration;
            float maxSpeedChange = acc * Time.fixedDeltaTime;

            var direction = move;
            if (playerInputSpace)
            {
                Vector3 forward = playerInputSpace.forward;
                forward.y = 0f; forward.Normalize();
                Vector3 right = playerInputSpace.right;
                right.y = 0f; right.Normalize();
                var relDir = forward * direction.y + right * direction.x;
                direction = new(relDir.x, relDir.z);
            }
            float newX = Mathf.MoveTowards(currentX, speed * direction.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, speed * direction.y, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }
        Vector3 ProjectOnContactPlane(Vector3 vector) => vector - contactNormal * Vector3.Dot(vector, contactNormal);
        void ClearState()
        {
            groundContactCount = steepContactCount = 0;
            contactNormal = steepNormal = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                ++groundContactCount;
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                ++steepContactCount;
                steepNormal += normal;
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
        wantToJump = 0.2f;
    }

#pragma warning restore IDE0051

    #endregion InputSystem
}