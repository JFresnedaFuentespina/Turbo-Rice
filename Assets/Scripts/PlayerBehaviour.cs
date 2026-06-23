using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float velocity = 3f;
    public float rotationSpeed = 160f;
    public float jumpForce = 12f;
    public Transform cameraTarget;
    private float inputH;
    private float inputV;
    private bool jumpPressed;

    private Rigidbody rb;
    private Animator animator;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        // 1. dirección de cámara SIEMPRE fija en este tick
        Vector3 camForward = cameraTarget.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTarget.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 inputDir = camForward * inputV + camRight * inputH;

        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        // 2. mover primero (SIN depender de rotación del rigidbody)
        rb.MovePosition(rb.position + inputDir * velocity * Time.fixedDeltaTime);

        // 3. rotar hacia dirección objetivo (post-movimiento, estable)
        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);

            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRot, 12f * Time.fixedDeltaTime)
            );
        }

        animator.SetFloat("Speed", inputDir.magnitude);

        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
    }
}