using System;
using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    private static readonly int ShootTrigger = Animator.StringToHash("ShootTrigger");

    /// <summary>
    ///
    ///     Handles launching rockets
    /// 
    /// </summary>
    
    
    
    [SerializeField] private Transform rocketSpawnPoint;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float rocketCooldown;
    [SerializeField] private Animator wandAnimator;
    [SerializeField] private ParticleSystem reloadVFX;
    private PlayerInput playerInput;
    private bool canFireRocket = true;

    

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.GetRocketInput() && canFireRocket)
        {
           Instantiate(rocketPrefab, rocketSpawnPoint.position, rocketSpawnPoint.rotation);
           StartCoroutine(RocketCooldown());
           wandAnimator.SetTrigger(ShootTrigger);
        }
    }

    private IEnumerator RocketCooldown()
    {
        canFireRocket = false;
        yield return new WaitForSeconds(rocketCooldown);
        reloadVFX.Play();
        canFireRocket = true;
    }
}
