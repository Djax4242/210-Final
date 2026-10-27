using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoomerangThrowing : MonoBehaviour
{
    /// <summary>
    ///
    ///     Handles the throwing of the boomerang
    /// 
    /// </summary>
    
    
    
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject splittingRang;
    [SerializeField] private Transform boomerangContainer;
    [SerializeField] private Transform boomerangReference;
    [SerializeField] private float throwForce;
    [SerializeField] private Transform throwDirection;
    private bool isBoomerangOut;


    private void OnEnable()
    {
        PlayerEvents.OnBoomerangCollected += BoomerangCollected;
    }

    private void OnDisable()
    {
        PlayerEvents.OnBoomerangCollected -= BoomerangCollected;
    }
    
    private void Update()
    {
        if (isBoomerangOut) return;
        
        if (playerInput.GetInteractInput())
        {
            splittingRang.transform.SetParent(boomerangContainer, true);
            Rigidbody splittingRangRb = splittingRang.GetComponent<Rigidbody>();
            splittingRangRb.interpolation = RigidbodyInterpolation.Interpolate;
            PlayerEvents.BoomerangThrown();
            splittingRangRb.AddForce(throwDirection.transform.forward * throwForce, ForceMode.Impulse);

            isBoomerangOut = true;
        }
    }

    private void BoomerangCollected() => isBoomerangOut = false;
}
