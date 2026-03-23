using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 2.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private InputSystem_Actions controls;
    private Vector2 moveInput;

    public float pushForce = 2.5f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();

        // Replace "Player" with your actual Action Map name if different
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -1f;
        }

        float x = moveInput.x;
        float z = moveInput.y;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Replace "Player" and "Jump" with your actual map/action names
        if (controls.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
{
    Rigidbody body = hit.collider.attachedRigidbody;

    // Only push non-kinematic rigidbodies
    if (body == null || body.isKinematic)
        return;

    // Ignore downward hits so gravity/ground contacts don't push
    if (hit.moveDirection.y < -0.3f)
        return;

    // Push only along XZ plane (no vertical launch)
    Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

    // Use velocity change for snappy arcade-style bumping
    body.AddForce(pushDir * pushForce, ForceMode.VelocityChange);
}
}