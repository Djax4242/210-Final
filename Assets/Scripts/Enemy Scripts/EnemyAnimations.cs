using System;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour
{
    [SerializeField] private Transform eye;
    [SerializeField] private Transform spinny1;
    [SerializeField] private Transform spinny2;
    [SerializeField] private Transform spinny3;
    [SerializeField] private Transform spinny4;
    [SerializeField] private float spinnyRotationSpeed;
    [SerializeField] private Transform spinnyRoot; 
    private GameObject _player;

    

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        eye.transform.LookAt(_player.transform.position);
        
        spinny1.Rotate(0, 0, spinnyRotationSpeed * Time.deltaTime);
        spinny2.Rotate(0, spinnyRotationSpeed  * Time.deltaTime, 0);
        spinny3.Rotate(0, 0, -spinnyRotationSpeed  * Time.deltaTime);
        spinny4.Rotate(0, -spinnyRotationSpeed  * Time.deltaTime, 0);
    }
}
