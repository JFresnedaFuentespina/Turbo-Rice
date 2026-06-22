using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float velocity = 3f;
    public float rotationSpeed = 160f;
    public float jumpForce = 12f;

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");

        // MOVIMIENTO
        Vector3 movement = transform.forward * inputV;
        movement *= velocity * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // ROTACIÓN
        if (Mathf.Abs(inputH) > 0.01f)
        {
            Quaternion rotation = Quaternion.Euler(
                0f,
                inputH * rotationSpeed * Time.fixedDeltaTime,
                0f
            );

            rb.MoveRotation(rb.rotation * rotation);
        }

        // ANIMACIÓN
        animator.SetFloat("Speed", Mathf.Abs(inputV));

        // SALTO
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}