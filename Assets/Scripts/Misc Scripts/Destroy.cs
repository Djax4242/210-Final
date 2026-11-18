using System;
using System.Collections;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] private float time;

    private void Awake() => StartCoroutine(DestroyTimer());

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    
}
