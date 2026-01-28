using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class FurniturePhysicsState : MonoBehaviour
{
    Rigidbody rb;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    bool isSettling;
    Vector3 lastPosition;
    Quaternion lastRotation;

    [Header("Settling")]
    [SerializeField] float settleVelocity = 0.05f;
    [SerializeField] float settleAngularVelocity = 0.05f;
    [SerializeField] float stableTime = 0.3f;

    float stableTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);

        SetPlacedState();
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    void Update()
    {
        if (!isSettling) return;

        // проверяем, "успокоился" ли объект
        if (rb.linearVelocity.magnitude < settleVelocity &&
            rb.angularVelocity.magnitude < settleAngularVelocity)
        {
            stableTimer += Time.deltaTime;

            if (stableTimer >= stableTime)
            {
                SetPlacedState();
                isSettling = false;
            }
        }
        else
        {
            stableTimer = 0f;
        }
    }

    // === STATES ===

    void OnGrabbed(SelectEnterEventArgs args)
    {
        SetGrabbedState();
        isSettling = false;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        StartSettling();
    }

    void StartSettling()
    {
        Debug.Log("START SETTLING");
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        isSettling = true;
        stableTimer = 0f;
    }

    public void SetPlacedState()
    {
        Debug.Log("Placed");
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void SetGrabbedState()
    {
        Debug.Log("GRABBED");
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
    }
}
