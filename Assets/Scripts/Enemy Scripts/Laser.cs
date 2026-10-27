using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float laserSpeed;
    [SerializeField] private Rigidbody laserRb;

    private void Awake()
    {
        laserRb.linearVelocity = transform.forward * laserSpeed;
    }
}
