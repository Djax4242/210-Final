using System;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    /// <summary>
    ///
    ///     Handles the rocket and the knockback that it does
    /// 
    /// </summary>
    
    
    
    [SerializeField] private float rocketSpeed;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackRadius;
    [SerializeField] private float upwardsMultiplier;
    private Rigidbody rocketRb;

    
    
    private void Awake()
    {
        rocketRb = GetComponent<Rigidbody>();
        rocketRb.linearVelocity = transform.forward * rocketSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] foundColliders = Physics.OverlapSphere(transform.position, knockbackRadius);

        foreach (var collider in foundColliders)
        {
            if (collider.TryGetComponent(out Rigidbody playerRb))
            {
                playerRb.AddExplosionForce(knockbackForce, transform.position, knockbackRadius, upwardsMultiplier, ForceMode.VelocityChange);
                break;
            }
        }
        
        Destroy(gameObject);
    }
}
