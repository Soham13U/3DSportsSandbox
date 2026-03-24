using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 7.0f;

    public float sprintSpeed = 10.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 2.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private InputSystem_Actions controls;
    private Vector2 moveInput;

    public float pushForce = 2.5f;

    public float kickRange = 2.0f;
    public float kickForce = 8.0f;
    public float kickUpwardBoost = 1.0f;

    public float kickCoolDown = 0.2f;
    public LayerMask ballLayer = ~0;

    private float nextKickTime;

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
        bool isSprinting = controls.Player.Sprint.IsPressed();
        bool hasMoveInput = moveInput.sqrMagnitude > 0.01f;
        float currentSpeed = (isSprinting && isGrounded && hasMoveInput) ? sprintSpeed : speed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);


        if (controls.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Persistent aim debug ray for kick direction/range visualization.
        Vector3 aimOrigin = GetCapsuleBottomWorldPoint();
        Vector3 aimDirection = GetKickAimDirection(aimOrigin);
        Debug.DrawRay(aimOrigin, aimDirection * kickRange, Color.red);

        if (controls.Player.Attack.triggered && Time.time >= nextKickTime)
        {
            nextKickTime = Time.time + kickCoolDown;

            Vector3 origin = aimOrigin;
            Vector3 direction = aimDirection;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, kickRange, ballLayer))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    Vector3 kickDir = direction.normalized + Vector3.up * kickUpwardBoost;
                    rb.AddForce(kickDir.normalized * kickForce, ForceMode.Impulse);
                }
            }

            Debug.DrawRay(origin, direction * kickRange, Color.yellow, 0.2f);
        }
    }

    private Vector3 GetCapsuleBottomWorldPoint()
    {
        Vector3 worldCenter = transform.TransformPoint(controller.center);
        float bottomOffset = (controller.height * 0.5f) - controller.radius;
        return worldCenter - Vector3.up * bottomOffset;
    }

    private Vector3 GetKickAimDirection(Vector3 origin)
    {
        Collider[] hits = Physics.OverlapSphere(origin, kickRange, ballLayer, QueryTriggerInteraction.Ignore);
        float closestSqrDist = float.MaxValue;
        Vector3 bestDirection = transform.forward;

        for (int i = 0; i < hits.Length; i++)
        {
            Rigidbody rb = hits[i].attachedRigidbody;
            if (rb == null || rb.isKinematic)
                continue;

            Vector3 toBall = rb.worldCenterOfMass - origin;
            float sqrDist = toBall.sqrMagnitude;
            if (sqrDist < closestSqrDist && sqrDist > 0.0001f)
            {
                closestSqrDist = sqrDist;
                bestDirection = toBall.normalized;
            }
        }

        return bestDirection;
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