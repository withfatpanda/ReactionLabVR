using UnityEngine;
using Oculus.Interaction;

// This script listens for the release (unselect) event on a Grabbable,
// and when triggered, moves the parent object of the Grabbable back to its original position and rotation.
// Attach this script to the child object that has the Grabbable and HandGrabInteractable components.
[RequireComponent(typeof(Grabbable))]
public class ResetParentOnRelease : MonoBehaviour
{
    // The original world position and rotation of the parent object
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    // Reference to the transform and rigidbody of the parent object (the object being moved)
    private Transform _targetTransform;
    private Rigidbody _targetRigidbody;

    // Reference to the Grabbable component on this child
    private Grabbable _grabbable;

    private void Awake()
    {
        // Set the target transform to be the parent of this object (the grabbed object)
        _targetTransform = transform.parent;

        // Record the original starting position and rotation of the target
        _originalPosition = _targetTransform.position;
        _originalRotation = _targetTransform.rotation;

        // Get the Grabbable component on this GameObject
        _grabbable = GetComponent<Grabbable>();

        // Try to get a Rigidbody from the parent object (optional, but used to reset velocity)
        _targetRigidbody = _targetTransform.GetComponent<Rigidbody>();

        // Subscribe to pointer event notifications (like select/unselect/move)
        _grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks or errors if object is destroyed
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    // Called every time the Grabbable receives a pointer event (select, unselect, move, etc.)
    private void OnPointerEvent(PointerEvent evt)
    {
        // Only respond to the Unselect event (i.e., when the object is released)
        if (evt.Type == PointerEventType.Unselect)
        {
            // Stop all physics movement if Rigidbody exists, so it doesn't keep drifting
            if (_targetRigidbody != null)
            {
                _targetRigidbody.velocity = Vector3.zero;
                _targetRigidbody.angularVelocity = Vector3.zero;
            }

            // Instantly move the object back to where it was at the start
            _targetTransform.position = _originalPosition;
            _targetTransform.rotation = _originalRotation;
        }
    }
}
