using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerTPS : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTarget;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;

    private float velocityY;
    private bool jumpPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // INPUT
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(inputH, inputV);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        // DEADZONE (CLAVE PARA EVITAR MOVIMIENTO INVISIBLE)
        if (input.magnitude < 0.1f)
            input = Vector2.zero;

        // DIRECCIÓN RELATIVA A CÁMARA
        Vector3 camForward = cameraTarget.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTarget.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 move = camForward * input.y + camRight * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // MOVIMIENTO
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ROTACIÓN SOLO SI HAY INPUT REAL
        if (input.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // GRAVEDAD
        if (controller.isGrounded && velocityY < 0)
            velocityY = -2f;

        if (jumpPressed && controller.isGrounded)
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }

        velocityY += gravity * Time.deltaTime;
        controller.Move(Vector3.up * velocityY * Time.deltaTime);

        // ANIMACIÓN (IMPORTANTE: SOLO INPUT, NO MOVIMIENTO FINAL)
        animator.SetFloat("MoveX", input.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", input.y, 0.1f, Time.deltaTime);

        animator.SetBool("Grounded", controller.isGrounded);
    }
}