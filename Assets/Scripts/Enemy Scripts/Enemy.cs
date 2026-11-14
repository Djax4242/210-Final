using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float startingHealth;
    private Transform _playerTransform;
    private float _enemyHealth;
    
    private void Awake()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _enemyHealth = startingHealth;
    }

    private void Update()
    {
        Vector3 lookTarget = new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z);
        Vector3 lookDirection = lookTarget - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed);
    }

    public void DealDamage(float damageAmount)
    {
        _enemyHealth -= damageAmount;
        if (_enemyHealth <= 0) KillEnemy();
    }

    public void KillEnemy()
    {
        Destroy(gameObject);
    }
}
