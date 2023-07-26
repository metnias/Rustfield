using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRotator : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rBody = null;

    [SerializeField, Range(-60f, 60f)]
    private float rotateX = 0f, rotateY = 0f, rotateZ = 0f;

    private void FixedUpdate()
    {
        if (!rBody) return;

        Quaternion dRot = Quaternion.Euler(rotateX * Time.fixedDeltaTime * 36f, rotateY * Time.fixedDeltaTime * 36f, rotateZ * Time.fixedDeltaTime * 36f);
        rBody.MoveRotation(rBody.rotation * dRot);
    }
}
