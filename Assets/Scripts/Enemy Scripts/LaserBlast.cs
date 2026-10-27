using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LaserBlast : MonoBehaviour
{
    [SerializeField] private Transform eye;
    [SerializeField] private float minBlastDelay;
    [SerializeField] private float maxBlastDelay;
    [SerializeField] private GameObject laser;
    

    private void Awake()
    {
        StartCoroutine(Laser());
    }

    private IEnumerator Laser()
    {
        yield return new WaitForSeconds(Random.Range(minBlastDelay, maxBlastDelay));
        Quaternion lookRotation = Quaternion.LookRotation(eye.transform.forward);
        Instantiate(laser, eye.position, lookRotation);
        StartCoroutine(Laser());
    }
}
