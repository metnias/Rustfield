using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.IMGUI.Controls.CapsuleBoundsHandle;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rBody;

    [SerializeField]
    private Transform playerInputSpace = default;

    [Header("Run")]
    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 1f;

    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 2f;

    [SerializeField]
    private LayerMask standableMask = -1;

    [Header("Jump")]
    [SerializeField, Range(0f, 100f)]
    private float jumpStrength = 10f;

    [SerializeField, Range(0f, 90f)]
    private float groundMaxAngle = 60f;

    [SerializeField, Range(0f, 100f)]
    private float airMaxAcceleration = 1f;

    [SerializeField, Range(0f, 20f)]
    private float gravity = 1f;

    [Header("Climb")]
    [SerializeField, Range(0f, 100f)]
    private float climbMaxSpeed = 1f;

    [SerializeField, Range(0f, 100f)]
    private float climbMaxAcceleration = 1f;

    [SerializeField, Range(90, 180)]
    private float climbMaxAngle = 140f;

    private Vector2 moveInput = Vector2.zero;
    private Rigidbody connectedBody, previousConnectedBody;
    private Vector3 connectionVelocity, connectionWorldPosition, connectionLocalPosition;
    private float wantToJump = 0f;
    private bool OnGround => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;
    private bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;
    private float minGroundDotProduct, minClimbDotProduct;
    private Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;
    private int groundContactCount = 0, steepContactCount = 0, climbContactCount;
    private int stepsSinceLastGrounded = 0, stepsSinceLastJump = 0;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(groundMaxAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(climbMaxAngle * Mathf.Deg2Rad);
    }

    private void Start()
    {
        OnValidate();
    }

    private void FixedUpdate()
    {
        Vector3 upAxis = -Physics.gravity.normalized;
        Vector3 velocity = rBody.velocity;
        UpdateState();

        if (Climbing) velocity -= contactNormal * (climbMaxAcceleration * 0.9f * Time.fixedDeltaTime);
        else if (OnGround && velocity.sqrMagnitude < 0.01f) velocity += contactNormal * (Vector3.Dot(Physics.gravity, contactNormal) * Time.fixedDeltaTime);
        else velocity += gravity * Time.fixedDeltaTime * Physics.gravity;

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
            if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts())
            {
                stepsSinceLastGrounded = 0;
                contactNormal.Normalize();
            }
            else
            {
                ++stepsSinceLastGrounded;
                contactNormal = upAxis;
            }
            ++stepsSinceLastJump;
            if (connectedBody)
                if (connectedBody.isKinematic || connectedBody.mass >= rBody.mass)
                    UpdateConnectionState();

            bool CheckClimbing()
            {
                if (!Climbing) return false;

                if (climbContactCount > 1)
                {
                    climbNormal.Normalize();
                    float upDot = Vector3.Dot(upAxis, climbNormal);
                    if (upDot >= minGroundDotProduct) climbNormal = lastClimbNormal;
                }
                groundContactCount = 1;
                contactNormal = climbNormal;
                return true;
            }
            void UpdateConnectionState()
            {
                if (connectedBody == previousConnectedBody)
                {
                    Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
                    connectionVelocity = connectionMovement / Time.deltaTime;
                }
                connectionWorldPosition = rBody.position;
                connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
            }
        }
        bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) return false;
            float curSpeed = velocity.magnitude;
            if (curSpeed > maxSpeed * 0.8f) return false;
            if (!Physics.Raycast(rBody.position, -upAxis, out RaycastHit hit, 0.5f, standableMask)) return false;
            if (hit.normal.y < minGroundDotProduct) return false;
            contactNormal = hit.normal;
            float newSpeed = velocity.magnitude;
            float dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0f) velocity = (velocity - hit.normal * dot).normalized * newSpeed;
            connectedBody = hit.rigidbody;
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
            jumpDir = (jumpDir + upAxis).normalized;

            stepsSinceLastJump = 0; wantToJump = 0f;
            float jumpSpeed = jumpStrength;
            float alignedSpeed = Vector3.Dot(velocity, jumpDir);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            velocity += jumpDir * jumpSpeed;
        }
        void AdjustVelocity()
        {
            float acceleration, speed;
            Vector3 xAxis, zAxis;
            if (Climbing)
            {
                acceleration = climbMaxAcceleration;
                speed = climbMaxSpeed;
                xAxis = Vector3.Cross(contactNormal, upAxis);
                zAxis = upAxis;
            }
            else
            {
                acceleration = OnGround ? maxAcceleration : airMaxAcceleration;
                speed = maxSpeed;
                xAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
                zAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }
            xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);
            zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);

            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);
            float maxSpeedChange = acceleration * Time.fixedDeltaTime;

            var direction = moveInput;
            if (!Climbing && playerInputSpace)
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
        Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) => (direction - normal * Vector3.Dot(direction, normal)).normalized;
        void ClearState()
        {
            groundContactCount = steepContactCount = climbContactCount = 0;
            contactNormal = steepNormal = climbNormal = connectionVelocity = Vector3.zero;
            previousConnectedBody = connectedBody;
            connectedBody = null;
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
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (normal.y > -0.01f)
                {
                    ++steepContactCount;
                    steepNormal += normal;
                    if (groundContactCount == 0) connectedBody = collision.rigidbody;
                }
                if (normal.y >= minClimbDotProduct)
                {
                    ++climbContactCount;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    #region InputSystem

#pragma warning disable IDE0051

    private void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void OnJump(InputValue _)
    {
        wantToJump = 0.2f;
    }

#pragma warning restore IDE0051

    #endregion InputSystem
}