using System.Collections;
using UnityEngine;

public class TorchThrowing : MonoBehaviour
{
    /// <summary>
    ///
    ///     Throws the held torch as a spinning physics object and hands the player a new one after a delay.
    ///     Also bobs the held torch while walking. Thrown torches are never despawned.
    ///
    /// </summary>


    [Header("--- References ---")]
    [Tooltip("Gets input")]
    [SerializeField] private PlayerInput playerInput;
    [Tooltip("Player rb, its velocity gets added to the throw")]
    [SerializeField] private Rigidbody playerRb;
    [Tooltip("Used to only bob while grounded")]
    [SerializeField] private PlayerMovement playerMovement;
    [Tooltip("The torch currently held under the camera")]
    [SerializeField] private GameObject heldTorch;
    [Tooltip("Thrown torches get parented here to keep the hierarchy clean. Leave empty for scene root")]
    [SerializeField] private Transform thrownTorchContainer;

    [Space]
    [Header("--- Throw Settings ---")]
    [Tooltip("Speed the torch leaves the hand at")]
    [SerializeField] private float throwSpeed = 22f;
    [Tooltip("Extra upward speed so the torch arcs")]
    [SerializeField] private float upwardSpeed = 3f;
    [Tooltip("End over end spin speed in radians per second")]
    [SerializeField] private float spinSpeed = 15f;
    [Tooltip("How far ahead to aim when the crosshair isn't on anything")]
    [SerializeField] private float maxAimDistance = 100f;
    [Tooltip("Half the torch's length, used to check it isn't inside a wall when released")]
    [SerializeField] private float releaseHalfLength = 0.9f;
    [Tooltip("Torch thickness used to check it isn't inside a wall when released")]
    [SerializeField] private float releaseRadius = 0.2f;
    [Tooltip("When the crosshair is on something closer than this, the throw gets softer so it doesn't bounce back at the player")]
    [SerializeField] private float closeThrowDistance = 5f;
    [Tooltip("Softest a close throw can get, as a fraction of the full throw")]
    [SerializeField] private float minCloseThrowStrength = 0.15f;

    [Space]
    [Header("--- Respawn Settings ---")]
    [Tooltip("Seconds until the player gets a new torch")]
    [SerializeField] private float respawnDelay = 1.5f;
    [Tooltip("How long the new torch takes to rise into the hand")]
    [SerializeField] private float raiseDuration = 0.2f;
    [Tooltip("Local offset the new torch rises from")]
    [SerializeField] private Vector3 raiseOffset = new Vector3(0f, -0.8f, 0f);

    [Space]
    [Header("--- Bob Settings ---")]
    [Tooltip("Bob cycle speed per meter walked. Higher means quicker steps")]
    [SerializeField] private float bobSpeed = 1.2f;
    [Tooltip("How far the torch bobs up and down")]
    [SerializeField] private float bobHeight = 0.05f;
    [Tooltip("How far the torch sways side to side")]
    [SerializeField] private float bobSway = 0.035f;
    [Tooltip("How many degrees the torch tilts with each step")]
    [SerializeField] private float bobTilt = 2f;
    [Tooltip("Horizontal speed where the bob is at full strength")]
    [SerializeField] private float fullBobSpeed = 5f;
    [Tooltip("How quickly the bob fades in and out when starting or stopping")]
    [SerializeField] private float bobBlendSpeed = 10f;

    private GameObject torchTemplate;
    private Vector3 heldLocalPosition;
    private Quaternion heldLocalRotation;
    private Collider[] playerColliders;
    private int releaseMask;
    private bool isHoldingTorch;
    private int thrownCount;
    private Vector3 currentRaiseOffset;
    private float bobPhase;
    private float bobWeight;



    private void Awake()
    {
        heldLocalPosition = heldTorch.transform.localPosition;
        heldLocalRotation = heldTorch.transform.localRotation;
        playerColliders = playerRb.GetComponentsInChildren<Collider>();
        if (playerMovement == null) playerMovement = playerRb.GetComponent<PlayerMovement>();
        releaseMask = ~(1 << playerRb.gameObject.layer);

        // Clone while inactive so the template never shows up in game
        bool wasActive = heldTorch.activeSelf;
        heldTorch.SetActive(false);
        torchTemplate = Instantiate(heldTorch, transform);
        torchTemplate.name = heldTorch.name + " (Template)";
        heldTorch.SetActive(wasActive);

        SetHeld(heldTorch);
        isHoldingTorch = true;
    }

    private void Update()
    {
        if (!isHoldingTorch) return;
    }

    private void LateUpdate()
    {
        UpdateBob();

        if (heldTorch == null) return;

        float stepSway = Mathf.Sin(bobPhase) * bobWeight;
        float stepBounce = Mathf.Sin(bobPhase * 2f) * bobWeight;
        Vector3 bobOffset = new Vector3(stepSway * bobSway, stepBounce * bobHeight, 0f);

        heldTorch.transform.localPosition = heldLocalPosition + currentRaiseOffset + bobOffset;
        heldTorch.transform.localRotation = heldLocalRotation * Quaternion.Euler(0f, 0f, -stepSway * bobTilt);
    }

    private void UpdateBob()
    {
        // Advance the bob by distance walked so the steps match how fast the player is moving
        Vector3 flatVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        float speed = flatVelocity.magnitude;
        bobPhase = (bobPhase + speed * bobSpeed * Time.deltaTime) % (Mathf.PI * 2f);

        float targetWeight = playerMovement.IsGrounded ? Mathf.Clamp01(speed / fullBobSpeed) : 0f;
        bobWeight = Mathf.Lerp(bobWeight, targetWeight, 1f - Mathf.Exp(-bobBlendSpeed * Time.deltaTime));
    }

    private void ThrowTorch()
    {
        GameObject torch = heldTorch;
        heldTorch = null;
        isHoldingTorch = false;

        torch.transform.SetParent(thrownTorchContainer, true);
        torch.name = $"Thrown Torch {++thrownCount}";

        // The held torch can poke into walls. Releasing it there would make physics shove it out, possibly into the player's face,
        // so pull it back towards the camera until it's clear
        Vector3 cameraPosition = transform.position;
        Vector3 releasePosition = torch.transform.position;
        for (float t = 1f; t >= 0f; t -= 0.1f)
        {
            Vector3 candidate = Vector3.Lerp(cameraPosition, releasePosition, t);
            Vector3 halfLength = torch.transform.up * releaseHalfLength;
            if (!Physics.CheckCapsule(candidate - halfLength, candidate + halfLength, releaseRadius, releaseMask, QueryTriggerInteraction.Ignore))
            {
                torch.transform.position = candidate;
                break;
            }
        }

        // Aim from the torch to whatever the crosshair is on so it lands where the player is looking
        Ray aimRay = new Ray(cameraPosition, transform.forward);
        bool aimHit = Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance, releaseMask, QueryTriggerInteraction.Ignore);
        Vector3 aimPoint = aimHit ? hit.point : aimRay.GetPoint(maxAimDistance);

        // Soften throws at things right in front of the player so the torch drops against them instead of rebounding.
        // Close aim points are off to the side of the hand, so lean the throw towards camera forward instead of across the screen
        float throwStrength = aimHit ? Mathf.Clamp(hit.distance / closeThrowDistance, minCloseThrowStrength, 1f) : 1f;
        Vector3 throwDirection = Vector3.Slerp(transform.forward, (aimPoint - torch.transform.position).normalized, throwStrength);

        // Thrown torches never collide with the player, in flight or after landing
        foreach (Collider torchCollider in torch.GetComponentsInChildren<Collider>(true))
        {
            torchCollider.enabled = true;
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(torchCollider, playerCollider);
            }
        }

        Rigidbody torchRb = torch.GetComponent<Rigidbody>();
        torchRb.isKinematic = false;
        torchRb.interpolation = RigidbodyInterpolation.Interpolate;
        torchRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        torchRb.linearVelocity = playerRb.linearVelocity + (throwDirection * throwSpeed + Vector3.up * upwardSpeed) * throwStrength;
        // Spin around the camera's right axis so the flame tumbles end over end, away from the player
        torchRb.angularVelocity = transform.right * (spinSpeed * throwStrength);

        StartCoroutine(GiveNewTorch());
    }

    private IEnumerator GiveNewTorch()
    {
        yield return new WaitForSeconds(respawnDelay);

        GameObject torch = Instantiate(torchTemplate, transform);
        torch.name = torchTemplate.name.Replace(" (Template)", "");
        SetHeld(torch);
        currentRaiseOffset = raiseOffset;
        heldTorch = torch;
        torch.SetActive(true);

        // Raise the new torch into the hand
        float timer = 0f;
        while (timer < raiseDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / raiseDuration);
            currentRaiseOffset = Vector3.Lerp(raiseOffset, Vector3.zero, t);
            yield return null;
        }
        currentRaiseOffset = Vector3.zero;

        isHoldingTorch = true;
    }

    private void SetHeld(GameObject torch)
    {
        torch.transform.SetParent(transform, false);
        torch.transform.localPosition = heldLocalPosition;
        torch.transform.localRotation = heldLocalRotation;

        // Held torches shouldn't bump into anything
        Rigidbody torchRb = torch.GetComponent<Rigidbody>();
        torchRb.isKinematic = true;
        torchRb.interpolation = RigidbodyInterpolation.None;
        foreach (Collider torchCollider in torch.GetComponentsInChildren<Collider>(true))
        {
            torchCollider.enabled = false;
        }
    }
}
