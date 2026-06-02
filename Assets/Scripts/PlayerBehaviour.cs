using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject body;
    private Rigidbody rb;

    [Header("Movimiento")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;

    [Header("Configuración de Inclinación")]
    [Tooltip("Grados de inclinación hacia atrás al alcanzar la velocidad máxima.")]
    public float maxLeanAngle = -30f;
    [Tooltip("Velocidad con la que se inclina al arrancar.")]
    public float leanSmoothSpeed = 15f;
    [Tooltip("Velocidad con la que regresa a la posición recta al detenerse.")]
    public float straightenSmoothSpeed = 10f;

    private float speed = 0f;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Capturar entradas del teclado
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 2. Calcular la dirección basada en los ejes correctos del PADRE (Player)
        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // 3. Controlar la velocidad lineal
        if (moveDirection.magnitude > 0)
        {
            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            speed = Mathf.MoveTowards(speed, 0f, acceleration * Time.deltaTime);
        }

        // 4. Rotación horizontal CORRECTA (El objeto PADRE gira limpiamente hacia donde camina)
        if (moveDirection != Vector3.zero)
        {
            // Usamos Atan2 estándar porque el padre 'Player' sí tiene la Z hacia adelante correctamente
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetLookRotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, Time.deltaTime * leanSmoothSpeed);
        }

        // 5. Inclinación matemática pura aplicada al eje visual real (Z local de Body_Top)
        // Usamos el signo original de 'maxLeanAngle' configurado en el inspector
        float targetLean = (speed / maxSpeed) * maxLeanAngle;

        // Extraer el ángulo Z local actual de tu objeto Body_Top
        float currentZRotation = body.transform.localEulerAngles.z;
        if (currentZRotation > 180) currentZRotation -= 360;

        // Suavizar la transición numérica del ángulo
        float stepSpeed = (moveDirection.magnitude > 0 ? leanSmoothSpeed : straightenSmoothSpeed) * 10f;
        float newZRotation = Mathf.MoveTowards(currentZRotation, targetLean, stepSpeed * Time.deltaTime);

        // Mantenemos las rotaciones originales de X e Y de Body_Top para que no se descuadre,
        // y solo modificamos de forma aislada el eje Z que lo inclina hacia atrás.
        body.transform.localRotation = Quaternion.Euler(body.transform.localEulerAngles.x, body.transform.localEulerAngles.y, newZRotation);
    }

    void FixedUpdate()
    {
        // 6. Aplicar la velocidad calculada directamente al Rigidbody del objeto padre
        Vector3 targetVelocity = moveDirection * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}
