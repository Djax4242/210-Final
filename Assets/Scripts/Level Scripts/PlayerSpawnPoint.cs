using System;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{   
    [Header("=== DEBUG ===")]
    [SerializeField] private float gizmoSphereRadius;
    [SerializeField] private float gizmoRayDistance;
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, gizmoSphereRadius);
        Gizmos.DrawRay(transform.position, Vector3.up * gizmoRayDistance);
    }
}
