using System;
using System.Collections;
using UnityEngine;

public class EnemyHeallth : MonoBehaviour
{
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private GameObject[] spinnies;
    [SerializeField] private Transform bodyParts;
    
    
    private bool _canBeHit = true;
    private int _currentHealth = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (!_canBeHit) return;
        
        if (other.gameObject.CompareTag("Boomerang"))
        {
            print(_currentHealth);
            
            Rigidbody spinnyRb = spinnies[_currentHealth - 1].AddComponent<Rigidbody>();
            spinnyRb.interpolation = RigidbodyInterpolation.Interpolate;
            spinnyRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            spinnyRb.mass = 5f;
            spinnies[_currentHealth - 1].AddComponent<BoxCollider>();
            if (bodyParts == null) bodyParts = GameObject.FindGameObjectWithTag("BodyParts").transform;
            spinnies[_currentHealth - 1].transform.SetParent(bodyParts, true);
            
            _currentHealth--;
            _canBeHit = false;
            StartCoroutine(Invincibility());
        }

        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Invincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        _canBeHit = true;
    }
}
