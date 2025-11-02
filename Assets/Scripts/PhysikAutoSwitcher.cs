using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysikAutoSwitcher : MonoBehaviour
{
    public LayerMask groundMask;    
    public float groundCheckDistance = 0.1f;
    public bool startKinematic = true;

    private Rigidbody rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = startKinematic;
        rb.useGravity = !startKinematic;
    }

    void Update()
    {
        bool onGround = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (!isGrounded && onGround)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            isGrounded = true;
            Debug.Log($"{name} ist gelandet → Kinematic aktiviert");
        }
        else if (isGrounded && !onGround)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            isGrounded = false;
            Debug.Log($"{name} fällt → Kinematic deaktiviert");
        }
    }
}

