using System;
using System.Collections;
using UnityEngine;

public class BoomerangMovement : MonoBehaviour
{
    /// <summary>
    ///
    ///     Handles the boomerang movement when its thrown
    ///     Handles Visual rotation when the boomerang is thrown
    /// 
    /// </summary>
    
    
    [Header("--- References ---")]
    [Tooltip("Visual of the boomerang so we can rotate it")]
    [SerializeField] private Transform boomerangVisual;
    [Tooltip("Need this so we can disable the boomerangs collider when its held")]
    [SerializeField] private Collider collider;
    [Tooltip("This is where the boomerang will try to return to")]
    [SerializeField] private Transform holdPoint;
    [Tooltip("Rigidbody of the boomerang")]
    [SerializeField] private Rigidbody boomerangRb;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform boomerangReference;
    
    [Space]
    [Header("--- Boomerang Movement Settings ---")]
    [Tooltip("How fast the boomerang visual rotates")]
    [SerializeField] private float visualRotationSpeed;
    [Tooltip("How far away the boomerang needs to be to be collected")]
    [SerializeField] private float collectionDistance;
    [Tooltip("How aggressively the boomerang will try to return to the player")]
    [SerializeField] private float returnForce;
    [Tooltip("How long the boomerang cant be picked up")]
    [SerializeField] private float pickupInvulnerabilityTime;
    [SerializeField] private float returnDamping;
    [Tooltip("How long the collect snap/lerp takes")]
    [SerializeField] private float collectLerpDuration;
    // Is the boomerang currently out
    private bool isBoomerangOut;
    // Can the boomerang be picked up
    private bool canBoomerangBePickedUp;
    // How long the boomerang is out
    private int boomerangThrownTime;
    // Running snap back to the hand, tracked so a new throw cant start mid snap
    private Coroutine snapRoutine;
    
    
    
    private void OnEnable()
    {
        PlayerEvents.OnBoomerangThrown += Thrown;
    }

    private void OnDisable()
    {
        PlayerEvents.OnBoomerangThrown -= Thrown;
    }

    private void Update()
    {
        if (!isBoomerangOut) return;
        boomerangVisual.transform.Rotate(0, visualRotationSpeed * Time.deltaTime, 0, Space.Self);
    }

    private void FixedUpdate()
    {
        if (!isBoomerangOut) return;

        if (Vector3.Distance(boomerangRb.position, holdPoint.position) < collectionDistance && canBoomerangBePickedUp)
        {
            Collected();
            canBoomerangBePickedUp = false;
            return;
        }

        // Force the boomerang to try and return to the player
        Vector3 dirToTarget = holdPoint.position - boomerangRb.position;
        Vector3 boomerangAcceleration = dirToTarget * returnForce - boomerangRb.linearVelocity * returnDamping;
        boomerangRb.linearVelocity += boomerangAcceleration;
    }

    private void Thrown()
    {
        // Safety net, a throw should never land mid snap but if it does dont let
        // the snap keep running and drag the boomerang back into the hand
        if (snapRoutine != null)
        {
            StopCoroutine(snapRoutine);
            snapRoutine = null;
        }

        isBoomerangOut = true;
        collider.enabled = true;
        trailRenderer.enabled = true;
    
        boomerangRb.isKinematic = false;
        boomerangRb.constraints = RigidbodyConstraints.None;
    
        StartCoroutine(PickupInvulnerability());
    }

    private void Collected()
    {
        isBoomerangOut = false;
        collider.enabled = false;
        trailRenderer.enabled = false;

        boomerangRb.linearVelocity = Vector3.zero;
        boomerangRb.angularVelocity = Vector3.zero;
        boomerangRb.isKinematic = true;
        boomerangRb.interpolation = RigidbodyInterpolation.None;

        snapRoutine = StartCoroutine(SnapToHold());
    }

    private IEnumerator SnapToHold()
    {
        Vector3 startPos = boomerangRb.position;
        Quaternion startRot = boomerangRb.rotation;
        Quaternion startVisualRot = boomerangVisual.rotation;

        float t = 0f;
        while (t < collectLerpDuration)
        {
            t += Time.deltaTime;
            float linearT = t / collectLerpDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, linearT);

            transform.position = Vector3.Lerp(startPos, boomerangReference.position, easedT);
            boomerangRb.rotation = Quaternion.Slerp(startRot, boomerangReference.rotation, easedT);
            boomerangVisual.rotation = Quaternion.Slerp(startVisualRot, boomerangReference.rotation, easedT);

            yield return null;
        }

        transform.position = boomerangReference.position;
        boomerangRb.rotation = boomerangReference.rotation;
        boomerangVisual.rotation = boomerangReference.rotation;

        boomerangRb.transform.SetParent(holdPoint, true);
        boomerangRb.constraints = RigidbodyConstraints.FreezePosition;

        // Only tell the thrower its back in hand once the snap has finished, otherwise
        // holding the throw button fires a throw mid snap and the boomerang sticks to the hand
        snapRoutine = null;
        PlayerEvents.BoomerangCollected();
    }

    private IEnumerator PickupInvulnerability()
    {
        yield return new WaitForSeconds(pickupInvulnerabilityTime);
        canBoomerangBePickedUp = true;
    }
}