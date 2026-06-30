using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerTPS : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTarget;
    public Animator animator;
    private CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;
    public bool isRunning = false;

    [Header("Gravity")]
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    private float velocityY;

    [Header("Multipliers")]
    public float airSpeedMultiplier = 1.3f;
    public float runSpeedMultiplier = 1.5f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // INPUT
        Vector2 input = GetInput();

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            animator.SetTrigger("Jump");
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunning = false;
        }

        // DIRECCIÓN RELATIVA A CÁMARA
        Vector3 move = GetDirectionCamera(input);

        // ROTAR PERSONAJE
        RotatePlayer(input, move);

        // SALTO
        CheckJump();

        // ANIMACIÓN MOVIMIENTO
        AnimateMove(input);
    }

    Vector2 GetInput()
    {
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(inputH, inputV);

        // DEADZONE (CLAVE PARA EVITAR MOVIMIENTO INVISIBLE)
        if (input.magnitude < 0.1f)
            input = Vector2.zero;

        return input;
    }

    Vector3 GetDirectionCamera(Vector2 input)
    {
        Vector3 camForward = cameraTarget.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTarget.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 move = camForward * input.y + camRight * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        float speed = moveSpeed;

        if (!controller.isGrounded)
        {
            speed *= airSpeedMultiplier;
        }

        if (isRunning)
        {
            speed *= runSpeedMultiplier;
        }

        // MOVIMIENTO
        controller.Move(move * speed * Time.deltaTime);
        return move;
    }

    void RotatePlayer(Vector2 input, Vector3 move)
    {
        if (input.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void CheckJump()
    {
        // SALTO
        if (controller.isGrounded)
        {
            if (velocityY < 0)
                velocityY = -0.5f;
        }
        else
        {
            velocityY += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    void AnimateMove(Vector2 input)
    {
        animator.SetFloat("MoveX", input.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", input.y, 0.1f, Time.deltaTime);

        animator.SetBool("Grounded", controller.isGrounded);

    }
}