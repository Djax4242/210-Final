using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FallRespawn : MonoBehaviour
{
    /// <summary>
    ///
    ///     Trigger that sends the player back to where they started when they fall into it
    ///
    /// </summary>



    [Header("--- References ---")]
    [Tooltip("Rb on the player, its position when the scene starts is the respawn point")]
    [SerializeField] private Rigidbody playerRb;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;



    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        spawnPosition = playerRb.position;
        spawnRotation = playerRb.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the player respawns, thrown torches and anything else just fall in
        if (other.attachedRigidbody != playerRb) return;

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        // Set the transform too so interpolation doesn't smear the player across the level
        playerRb.position = spawnPosition;
        playerRb.rotation = spawnRotation;
        playerRb.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }
}
