using System;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    /// <summary>
    ///
    ///     Allows the player to pick up and move objects
    /// 
    /// </summary>
    
    
    [Header("--- References ---")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform camTransform;
    [SerializeField] private Transform pullPoint;
    [SerializeField] private LineRenderer interactionLine;

    [Header("--- Pick Up Settings ---")] 
    [Tooltip("There is a raycast that checks to see if the player is close enough to pick up objects")]
    [SerializeField] private float raycastDistance;
    [SerializeField] private float pullForce;
    [SerializeField] private LayerMask layersToIgnore;
    private bool _holdingObject;
    private GameObject _holdPosition;
    private Rigidbody _objectRigidbody;
    
    
    
    private void Update()
    {
        if (_holdingObject)
        {
            interactionLine.enabled = true;
            interactionLine.SetPosition(0, _holdPosition.transform.position);
            interactionLine.SetPosition(1, pullPoint.position);
        }
        else
        {
            interactionLine.enabled = false;

            if (_holdPosition != null)
            {
                Destroy(_holdPosition.gameObject);
                _holdPosition = null;
            }

            _objectRigidbody = null;
        }
    }

    private void FixedUpdate()
    {
        CheckForInteraction();
        PullObject();
    }

    private void PullObject()
    {
        if (_objectRigidbody != null)
        {
            Vector3 directionToPullPoint = pullPoint.position - _objectRigidbody.position;
            _objectRigidbody.AddForce(directionToPullPoint * (pullForce * directionToPullPoint.sqrMagnitude));
        }
    }

    private void CheckForInteraction()
    {
        if (!playerInput.GetInteractInput())
        {
            _holdingObject = false; 
            return;
        }
        
        if(Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hitInfo, raycastDistance, ~layersToIgnore) && !_holdingObject)
        {
            if (hitInfo.transform.gameObject.TryGetComponent(out Rigidbody objectRigidbody))
            {
                _holdPosition = new GameObject();
                _holdPosition.transform.position = hitInfo.point;
                _holdPosition.transform.SetParent(objectRigidbody.transform);

                _objectRigidbody = objectRigidbody;
                
                _holdingObject = true;
            }
        }
    }
}
