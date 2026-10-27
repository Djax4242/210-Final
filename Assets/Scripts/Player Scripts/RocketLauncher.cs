using System;
using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    /// <summary>
    ///
    ///     Handles launching rockets
    /// 
    /// </summary>
    
    
    
    [SerializeField] private Transform rocketSpawnPoint;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float rocketCooldown;
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
        }
    }

    private IEnumerator RocketCooldown()
    {
        canFireRocket = false;
        yield return new WaitForSeconds(rocketCooldown);
        canFireRocket = true;
    }
}
