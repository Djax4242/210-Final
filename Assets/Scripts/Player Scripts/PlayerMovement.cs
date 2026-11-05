using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    ///
    ///     First person player controller
    /// 
    /// </summary>
    
    
    [Header("--- References ---")]
    [Tooltip("Used top get cam relative input")]
    [SerializeField] private Transform mainCameraTransform;
    [Tooltip("Rb on the player")]
    [SerializeField] private Rigidbody playerRb;
    [Tooltip("Gets input")]
    [SerializeField] private PlayerInput playerInput;
    
    [Space]
    [Header("--- Movement Settings ---")]
    [Tooltip("How quickly input accelerates the player in the air")]
    [SerializeField] private float baseMoveSpeed;
    [Tooltip("Base speed gets multiplied by this when the player is trying to change direction")]
    [SerializeField] private float counterAccelerationMultiplier;
    [Tooltip("How fast grounded velocity blends to where the player is pressing with full grip. Around 250 is one physics step")]
    [SerializeField] private float fullGripBlendSpeed;
    [Tooltip("Same as above but with no grip, like right after landing. Lower means momentum lasts longer")]
    [SerializeField] private float lowGripBlendSpeed;
    [Tooltip("Seconds on the ground to go from no grip to full grip, and seconds in the air to lose it")]
    [SerializeField] private float gripBuildTime;
    [Tooltip("Grip fades as the player goes from top speed up to this speed, and they have none at or above it")]
    [SerializeField] private float noGripSpeed;
    [Tooltip("The player counts as airborne while rising faster than this, so ground movement doesn't slow rocket jumps")]
    [SerializeField] private float groundedRiseCutoff = 1f;
    [Tooltip("Friction applied when the player isn't grounded")]
    [SerializeField] private float airBorneFriction;
    [Tooltip("Top horizontal speed the player can reach with input alone. Rockets can push them past this")]
    [SerializeField] private float topSpeed;
    private float moveSpeed;
    private float grip = 1f;
    
    [Space]
    [Header("--- Jump Settings ---")]
    [Tooltip("How high the player jumps")]
    [SerializeField] private float jumpForce;
    [Tooltip("Max jumps the player has")]
    [SerializeField] private int maxJumps;
    [Tooltip("Extra gravity gets applied at the end")]
    [SerializeField] private float gravity;
    [Tooltip("Cooldown between jumps")]
    [SerializeField] private float jumpCooldown;
    [Tooltip("Grounded check wont use these layers")] 
    [SerializeField] private LayerMask invalidGroundedLayer;
    [SerializeField] private Vector3 spherecastOffset;
    [SerializeField] private float spherecastRadius;
    [SerializeField] private float spherecastDistance;
    private bool isGrounded = true;
    public bool IsGrounded => isGrounded;
    private int currentJumps;
    private bool canJump = true;
    
    
    
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void FixedUpdate()
    {
        Movement();
        Friction();
        
        CheckForJump();
        CheckForGrounded();
    }

    private void Movement()
    {
        // Get input
        Vector2 moveInput = playerInput.GetMoveInput();
        
        // Rotate the player rigidbody to face the cameras forward direction.
        // We flatten the Y to make sure the rigidbody doesn't tilt up and down
        Vector3 desiredForward = mainCameraTransform.forward;
        desiredForward.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(desiredForward);
        playerRb.MoveRotation(lookRotation);
        
        // After the rb is rotated, we use its foward and right to calulate an rb relative input
        Vector3 rbRelativeInput = (moveInput.x * playerRb.transform.right) + (moveInput.y * playerRb.transform.forward);
        
        // The grounded check takes a couple of physics steps to notice the player has left the ground,
        // so count them as airborne while they're rising or ground movement eats the speed from rocket jumps
        bool onGround = isGrounded && playerRb.linearVelocity.y <= groundedRiseCutoff;
        UpdateGrip(onGround);

        if (onGround)
        {
            GroundedMovement(rbRelativeInput);
        }
        else
        {
            AirborneMovement(rbRelativeInput);
        }

        // Extra gravity
        playerRb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void UpdateGrip(bool onGround)
    {
        // Grip builds up on the ground and fades in the air
        grip = Mathf.MoveTowards(grip, onGround ? 1f : 0f, Time.deltaTime / gripBuildTime);

        // Moving faster than the player can run also takes grip away, so any burst of speed carries over to the ground
        Vector3 flattenedVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        grip = Mathf.Min(grip, 1f - Mathf.InverseLerp(topSpeed, noGripSpeed, flattenedVelocity.magnitude));
    }

    private void GroundedMovement(Vector3 rbRelativeInput)
    {
        // Blend the players velocity towards where they're pressing. With full grip this is instant so
        // changing direction is snappy, with low grip the player keeps their momentum and eases into it
        Vector3 flattenedVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        Vector3 targetVelocity = Vector3.ClampMagnitude(rbRelativeInput, 1f) * topSpeed;
        float blendSpeed = Mathf.Lerp(lowGripBlendSpeed, fullGripBlendSpeed, grip * grip * grip);
        Vector3 newVelocity = Vector3.Lerp(flattenedVelocity, targetVelocity, 1f - Mathf.Exp(-blendSpeed * Time.deltaTime));
        playerRb.AddForce(newVelocity - flattenedVelocity, ForceMode.VelocityChange);
    }

    private void AirborneMovement(Vector3 rbRelativeInput)
    {
        // Work out how much the input would change the players velocity this physics step
        Vector3 flattenedVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        Vector3 inputVelocity = rbRelativeInput * (baseMoveSpeed * Time.deltaTime * Time.deltaTime / playerRb.mass);

        // Input can only speed the player up to top speed. If something else pushed them past it
        // (rocket jumps), input can steer and slow them down but wont add any more speed
        float speedLimit = Mathf.Max(topSpeed, flattenedVelocity.magnitude);
        Vector3 cappedVelocity = Vector3.ClampMagnitude(flattenedVelocity + inputVelocity, speedLimit);
        playerRb.AddForce(cappedVelocity - flattenedVelocity, ForceMode.VelocityChange);
    }

    private void Friction()
    {
        // Friction that gets applied in the air, ground movement handles slowing down on its own

        if (!isGrounded)
        {
            Vector3 flattenedVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
            playerRb.AddForce(-flattenedVelocity * (airBorneFriction * Time.deltaTime));
        }
    }

    private void CheckForJump()
    {
        if (!playerInput.GetJumpInput()) return;
        if (!canJump) return;
        
        currentJumps--;
        currentJumps = Mathf.Clamp(currentJumps, 0, maxJumps);
            
        if (currentJumps != 0)
        {
            playerRb.AddForce(Vector3.up * jumpForce);
            canJump = false;

            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
            
            StartCoroutine(JumpCooldown());
        }
    }

    private void CheckForGrounded()
    {
        if (Physics.SphereCast(playerRb.transform.position + spherecastOffset, spherecastRadius, Vector3.down, out RaycastHit hit, spherecastDistance, ~invalidGroundedLayer))
        {
            isGrounded = true;
            currentJumps = maxJumps; 
        }
        else
        {
            isGrounded = false;
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    
    
#if UNITY_EDITOR
    
    [Space]
    [Header("=== DEBUG SETTINGS ===")]
    [SerializeField] private bool showGizmos;
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (!Application.isPlaying) return;
        
        Vector2 moveInput = playerInput.GetMoveInput();
        Vector3 rbRelativeInput = (moveInput.x * playerRb.transform.right) + (moveInput.y * playerRb.transform.forward);
        
        Gizmos.color = Color.red;
        Vector3 spherePosition = playerRb.transform.position + spherecastOffset + new Vector3(0, -spherecastDistance, 0);
        Gizmos.DrawSphere(spherePosition, spherecastRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(playerRb.position, playerRb.linearVelocity);

        Gizmos.color = Color.orangeRed;
        Gizmos.DrawRay(playerRb.position, rbRelativeInput);
    }
#endif
}
