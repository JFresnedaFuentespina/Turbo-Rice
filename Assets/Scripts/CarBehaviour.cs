using UnityEngine;
using UnityEngine.InputSystem;

public class CarBehaviour : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction brakeAction;

    [Header("Componentes Ruedas")]
    public WheelCollider ruedaDelanteraIzquierda;
    public WheelCollider ruedaDelanteraDerecha;
    public WheelCollider ruedaTraseraIzquierda;
    public WheelCollider ruedaTraseraDerecha;
    public GameObject ruedaDelanteraIzquierdaGo;
    public GameObject ruedaDelanteraDerechaGo;
    public GameObject ruedaTraseraIzquierdaGo;
    public GameObject ruedaTraseraDerechaGo;

    [Header("Movimiento Avanzado")]
    public float torqueMotor = 3000f;
    public float velocidadMaxima = 45f;
    public float anguloMaximoGiro = 35f;
    public float fuerzaFrenoMano = 8000f;
    public float fuerzaDownforce = 100f;
    [Tooltip("Fuerza de arrastre aerodinámico directo que frena el chasis al soltar el acelerador (¡Recomendado: 0.5 a 2!)")]
    public float arrastreAerodinamicoArcade = 1.0f;


    [Header("Configuración de Derrape (Drift Arcade)")]
    [Range(0f, 1f)] public float friccionNormalLateral = 1.0f;
    [Range(0f, 1f)] public float friccionDerrapeLateral = 0.15f;
    public float suavizadoFriccion = 8f;

    [Tooltip("Fuerza que empuja el coche hacia adelante mientras derrapas para no perder velocidad")]
    public float impulsoArcadeDerrape = 1500f;

    [Tooltip("Ayuda al coche a pivotar de lado de forma exagerada y controlada")]
    public float asistenciaRotacionDerrape = 2.5f;

    [Header("Respuesta Inmediata Arcade")]
    [Tooltip("Fuerza para neutralizar la inercia al cambiar instantáneamente de marcha adelante/atrás")]
    public float fuerzaCambioMarchaInmediato = 2500f;

    [Tooltip("Fuerza extra para clavar el coche en seco al frenar en línea recta (sin derrapar)")]
    public float fuerzaFrenadoSecoLineal = 5000f;

    [Tooltip("Fuerza de frenado automática cuando el jugador suelta el acelerador")]
    public float frenoMotorArcade = 150f;

    [Header("Ajuste Visual Ruedas")]
    [Tooltip("Multiplicador para ajustar la velocidad de giro visual de los neumáticos si se ve muy rápido o lento")]
    public float multiplicadorGiroVisual = 1.0f;

    [Header("Configuración")]
    public Rigidbody rb;
    public Transform centroDeMasa;

    private Vector2 inputMovimiento;
    private float inputFreno;
    private float friccionActualTrasera;
    private bool estaDerrapando = false;
    private float anguloRodamientoRuedas = 0f;

    // Variables internas temporales para evitar conflictos físicos
    private float torqueMotorTraseroActual = 0f;
    private float frenoTraseroActual = 0f;
    private float frenoDelanteroActual = 0f;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (centroDeMasa != null)
        {
            rb.centerOfMass = transform.InverseTransformPoint(centroDeMasa.position);
        }

        moveAction = inputActions.FindAction("Move");
        moveAction.Enable();

        brakeAction = inputActions.FindAction("Brake");
        if (brakeAction != null) brakeAction.Enable();

        friccionActualTrasera = friccionNormalLateral;
    }

    void Update()
    {
        inputMovimiento = moveAction.ReadValue<Vector2>();
        inputFreno = brakeAction != null ? brakeAction.ReadValue<float>() : 0f;

        CalcularRotacionRuedas();
        ActualizarRuedasVisuales();
    }

    void FixedUpdate()
    {
        // Resetear las fuerzas lógicas del frame
        torqueMotorTraseroActual = 0f;
        frenoTraseroActual = 0f;
        frenoDelanteroActual = 0f;

        // Calcular los sistemas físicos autónomos
        ProcesarGiro();
        ProcesarMotor();
        ProcesarFrenoYDerrape();
        AplicarFisicasArcade();
        AplicarDownforce();

        // Aplicar de golpe los torques resultantes a las ruedas físicas (Evita sobreescrituras)
        ruedaTraseraIzquierda.motorTorque = torqueMotorTraseroActual;
        ruedaTraseraDerecha.motorTorque = torqueMotorTraseroActual;

        ruedaTraseraIzquierda.brakeTorque = frenoTraseroActual;
        ruedaTraseraDerecha.brakeTorque = frenoTraseroActual;
        ruedaDelanteraIzquierda.brakeTorque = frenoDelanteroActual;
        ruedaDelanteraDerecha.brakeTorque = frenoDelanteroActual;
    }

    private void ProcesarGiro()
    {
        float anguloGiro = inputMovimiento.x * anguloMaximoGiro;
        ruedaDelanteraIzquierda.steerAngle = anguloGiro;
        ruedaDelanteraDerecha.steerAngle = anguloGiro;
    }

    private void ProcesarMotor()
    {
        float velocidadActual = Vector3.Dot(rb.linearVelocity, transform.forward);

        // Control de límite de velocidad máxima
        if (Mathf.Abs(velocidadActual) > velocidadMaxima)
        {
            torqueMotorTraseroActual = 0f;
            return;
        }

        // --- SISTEMA DE CAMBIO DE MARCHA ULTRA RÁPIDO ---
        bool quiereIrAdelantePeroVaAtras = (inputMovimiento.y > 0.1f && velocidadActual < -1f);
        bool quiereIrAtrasPeroVaAdelante = (inputMovimiento.y < -0.1f && velocidadActual > 1f);

        if (quiereIrAdelantePeroVaAtras || quiereIrAtrasPeroVaAdelante)
        {
            frenoTraseroActual = fuerzaFrenoMano;
            torqueMotorTraseroActual = 0f;

            Vector3 direccionContrarrestar = transform.forward * inputMovimiento.y;
            rb.AddForce(direccionContrarrestar * fuerzaCambioMarchaInmediato, ForceMode.Force);
            return;
        }

        // --- CONTROL DE DESACELERACIÓN ARCADE SECA ---
        if (Mathf.Abs(inputMovimiento.y) < 0.1f)
        {
            torqueMotorTraseroActual = 0f;

            if (Mathf.Abs(velocidadActual) > 0.1f)
            {
                // Freno físico en las ruedas
                frenoTraseroActual = frenoMotorArcade;

                // Esto corta de raíz el deslizamiento infinito de las milésimas.
                Vector3 fuerzaArrastre = -rb.linearVelocity * arrastreAerodinamicoArcade;
                rb.AddForce(fuerzaArrastre, ForceMode.Acceleration);
            }
            return;
        }

        torqueMotorTraseroActual = inputMovimiento.y * torqueMotor;
    }


    private void ProcesarFrenoYDerrape()
    {
        estaDerrapando = (inputFreno > 0.1f && Mathf.Abs(inputMovimiento.x) > 0.2f);
        float velocidadActual = Vector3.Dot(rb.linearVelocity, transform.forward);

        if (inputFreno > 0.1f)
        {
            if (estaDerrapando)
            {
                // Al derrapar bloqueamos atrás (Sobreescribe el freno motor si fuese mayor)
                frenoTraseroActual = Mathf.Max(frenoTraseroActual, fuerzaFrenoMano);
                frenoDelanteroActual = 0f;
            }
            else
            {
                // Freno seco: Bloqueamos las 4 ruedas por igual
                frenoTraseroActual = Mathf.Max(frenoTraseroActual, fuerzaFrenoMano);
                frenoDelanteroActual = fuerzaFrenoMano;

                if (Mathf.Abs(velocidadActual) > 0.5f)
                {
                    Vector3 direccionFrenado = -rb.linearVelocity.normalized;
                    rb.AddForce(direccionFrenado * fuerzaFrenadoSecoLineal, ForceMode.Force);
                }
            }
        }

        // Modulación de fricción lateral
        if (estaDerrapando)
        {
            friccionActualTrasera = Mathf.Lerp(friccionActualTrasera, friccionDerrapeLateral, Time.fixedDeltaTime * suavizadoFriccion);
        }
        else
        {
            friccionActualTrasera = Mathf.Lerp(friccionActualTrasera, friccionNormalLateral, Time.fixedDeltaTime * suavizadoFriccion);
        }

        AjustarFriccionLateralRueda(ruedaTraseraIzquierda, friccionActualTrasera);
        AjustarFriccionLateralRueda(ruedaTraseraDerecha, friccionActualTrasera);
    }

    private void AplicarFisicasArcade()
    {
        if (!estaDerrapando) return;

        if (inputMovimiento.y > 0.1f)
        {
            rb.AddForce(transform.forward * impulsoArcadeDerrape, ForceMode.Force);
        }

        float direccionGiro = inputMovimiento.x;
        rb.AddTorque(transform.up * direccionGiro * asistenciaRotacionDerrape, ForceMode.Acceleration);
    }

    private void AjustarFriccionLateralRueda(WheelCollider rueda, float valorStiffness)
    {
        WheelFrictionCurve friccionLateral = rueda.sidewaysFriction;
        friccionLateral.stiffness = valorStiffness;
        rueda.sidewaysFriction = friccionLateral;
    }

    private void AplicarDownforce()
    {
        Vector3 fuerzaHaciaAbajo = -transform.up * fuerzaDownforce * rb.linearVelocity.magnitude;
        rb.AddForce(fuerzaHaciaAbajo);
    }

    private void CalcularRotacionRuedas()
    {
        float velocidadAvanzado = Vector3.Dot(rb.linearVelocity, transform.forward);
        float radioRueda = ruedaDelanteraIzquierda != null ? ruedaDelanteraIzquierda.radius : 0.35f;

        float deltaAngulo = (velocidadAvanzado / radioRueda) * Time.deltaTime * Mathf.Rad2Deg * multiplicadorGiroVisual;
        anguloRodamientoRuedas += deltaAngulo;

        if (anguloRodamientoRuedas > 360f) anguloRodamientoRuedas -= 360f;
        if (anguloRodamientoRuedas < -360f) anguloRodamientoRuedas += 360f;
    }

    private void ActualizarRuedasVisuales()
    {
        SincronizarRueda(ruedaDelanteraIzquierda, ruedaDelanteraIzquierdaGo, true);
        SincronizarRueda(ruedaDelanteraDerecha, ruedaDelanteraDerechaGo, true);
        SincronizarRueda(ruedaTraseraIzquierda, ruedaTraseraIzquierdaGo, false);
        SincronizarRueda(ruedaTraseraDerecha, ruedaTraseraDerechaGo, false);
    }

    private void SincronizarRueda(WheelCollider colliderFisico, GameObject mallaVisual, bool esDelantera)
    {
        if (colliderFisico == null || mallaVisual == null) return;

        Vector3 posicionFisica;
        Quaternion rotacionFisica;

        colliderFisico.GetWorldPose(out posicionFisica, out rotacionFisica);
        mallaVisual.transform.position = posicionFisica;

        float anguloDireccionY = esDelantera ? colliderFisico.steerAngle : 0f;
        mallaVisual.transform.rotation = transform.rotation * Quaternion.Euler(anguloRodamientoRuedas, anguloDireccionY, 0f);
    }
}