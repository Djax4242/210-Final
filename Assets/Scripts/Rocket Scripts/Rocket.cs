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
    [SerializeField] private GameObject rocketExplosion;
    [Space]
    private Rigidbody rocketRb;

    
    
    private void Awake()
    {
        rocketRb = GetComponent<Rigidbody>();
        rocketRb.linearVelocity = transform.forward * rocketSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool directHit = false;
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (collision.gameObject.TryGetComponent(out Enemy enemy))
            {
                directHit = true;
                enemy.KillEnemy();
            }
        }
        
        Collider[] foundColliders = Physics.OverlapSphere(transform.position, knockbackRadius);

        foreach (var collider in foundColliders)
        {
            if (collider.TryGetComponent(out Rigidbody playerRb))
            {
                playerRb.AddExplosionForce(knockbackForce, transform.position, knockbackRadius, upwardsMultiplier, ForceMode.VelocityChange);
            }

            if (directHit) continue;
            if (collider.gameObject.layer != LayerMask.NameToLayer("Enemy")) continue;
            if (!collider.gameObject.TryGetComponent(out Enemy enemy)) continue;
            
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
            float damage = knockbackRadius / distanceToEnemy;
                    
            enemy.DealDamage(damage);
        }

        Instantiate(rocketExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }
}
