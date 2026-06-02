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

    [Header("Movimiento Avanzado")]
    public float torqueMotor = 3000f;      // ¡Fuerza aumentada para salida explosiva!
    public float velocidadMaxima = 45f;    // Velocidad límite (en m/s, aprox 160 km/h)
    public float anguloMaximoGiro = 35f;   // Más ángulo para giros cerrados y divertidos
    public float fuerzaFrenoMano = 8000f;  // Fuerza para clavar las ruedas traseras
    public float fuerzaDownforce = 100f;    // Mantiene el coche pegado al suelo a alta velocidad

    [Header("Configuración")]
    public Rigidbody rb;
    public Transform centroDeMasa; // Objeto vacío para bajar el centro de gravedad

    void Start()
    {
        // Configuración de controles
        moveAction = inputActions.FindAction("Move");
        moveAction.Enable();

        // Buscamos una acción de freno (puedes mapearla a Espacio o el botón Sur del mando)
        brakeAction = inputActions.FindAction("Brake");
        if (brakeAction != null) brakeAction.Enable();

        if (rb == null) rb = GetComponent<Rigidbody>();

        // TRUCO CRÍTICO: Bajar el centro de masa evita que el coche vuelque al girar rápido
        if (centroDeMasa != null)
        {
            rb.centerOfMass = transform.InverseTransformPoint(centroDeMasa.position);
        }
        else
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0); // Ajuste automático si no asignas nada
        }
    }

    void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float acelerar = input.y;
        float girar = input.x;

        // Leer si el jugador está frenando (Freno de mano / Derrape)
        bool estaFrenando = false;
        if (brakeAction != null)
        {
            estaFrenando = brakeAction.ReadValue<float>() > 0.1f;
        }
        else
        {
            // Atajo por si no tienes la acción configurada en el Input Action: se activa con S si vas hacia adelante
            estaFrenando = (acelerar < 0 && Vector3.Dot(rb.linearVelocity, transform.forward) > 1f);
        }

        AplicarFuerzaSuelo();
        ControlarMotorYFrenos(acelerar, estaFrenando);
        ControlarDireccion(girar);
    }

    void ControlarMotorYFrenos(float acelerar, bool estaFrenando)
    {
        float velocidadActual = rb.linearVelocity.magnitude;

        // Detectar si el jugador quiere ir en reversa o frenar con el pedal normal (S / Gatillo Izquierdo)
        bool quiereIrMarchaAtras = acelerar < 0;
        bool seEstaMoviendoHaciaAdelante = Vector3.Dot(rb.linearVelocity, transform.forward) > 0.5f;

        // Si pulsa el freno de mano O quiere frenar usando la marcha atrás mientras avanza
        if (estaFrenando || (quiereIrMarchaAtras && seEstaMoviendoHaciaAdelante))
        {
            // 1. Cortamos por completo la fuerza de todos los motores de inmediato
            ruedaTraseraIzquierda.motorTorque = 0f;
            ruedaTraseraDerecha.motorTorque = 0f;
            ruedaDelanteraIzquierda.motorTorque = 0f;
            ruedaDelanteraDerecha.motorTorque = 0f;

            // 2. ¡Frenazo brutal en las 4 ruedas! (Aumentamos a fuerzaFrenoMano en todo el coche)
            ruedaTraseraIzquierda.brakeTorque = fuerzaFrenoMano;
            ruedaTraseraDerecha.brakeTorque = fuerzaFrenoMano;
            ruedaDelanteraIzquierda.brakeTorque = fuerzaFrenoMano; // Añadido frenado delantero
            ruedaDelanteraDerecha.brakeTorque = fuerzaFrenoMano;   // Añadido frenado delantero

            // 3. Truco arcade: Aplicamos una contrafuerza física directa para clavar el coche sin deslizar infinitamente
            rb.AddForce(-rb.linearVelocity * 0.5f, ForceMode.Acceleration);
            return; // Salimos del método para que no ejecute la lógica de aceleración
        }

        // LÓGICA DE MOVIMIENTO NORMAL (Aceleración y Marcha Atrás quieto)
        if (velocidadActual < velocidadMaxima)
        {
            // Quitamos todos los frenos para poder movernos libres
            ruedaTraseraIzquierda.brakeTorque = 0f;
            ruedaTraseraDerecha.brakeTorque = 0f;
            ruedaDelanteraIzquierda.brakeTorque = 0f;
            ruedaDelanteraDerecha.brakeTorque = 0f;

            // Tracción en las 4 ruedas (AWD)
            ruedaTraseraIzquierda.motorTorque = acelerar * torqueMotor;
            ruedaTraseraDerecha.motorTorque = acelerar * torqueMotor;
            ruedaDelanteraIzquierda.motorTorque = acelerar * (torqueMotor * 0.5f);
            ruedaDelanteraDerecha.motorTorque = acelerar * (torqueMotor * 0.5f);
        }
        else
        {
            // Límite de velocidad máxima alcanzado
            ruedaTraseraIzquierda.motorTorque = 0f;
            ruedaTraseraDerecha.motorTorque = 0f;
            ruedaDelanteraIzquierda.motorTorque = 0f;
            ruedaDelanteraDerecha.motorTorque = 0f;
        }

        // Freno de resistencia ligera (cuando el jugador no toca ningún input)
        if (Mathf.Abs(acelerar) < 0.05f)
        {
            ruedaTraseraIzquierda.brakeTorque = 300f;
            ruedaTraseraDerecha.brakeTorque = 300f;
            ruedaDelanteraIzquierda.brakeTorque = 150f;
            ruedaDelanteraDerecha.brakeTorque = 150f;
        }
    }


    void ControlarDireccion(float girar)
    {
        // Dirección dinámica: a más velocidad, las ruedas giran menos para evitar trompos incontrolables
        float factorVelocidad = Mathf.InverseLerp(0, velocidadMaxima, rb.linearVelocity.magnitude);
        float anguloActual = Mathf.Lerp(anguloMaximoGiro, anguloMaximoGiro * 0.4f, factorVelocidad);

        float anguloGiro = girar * anguloActual;
        ruedaDelanteraIzquierda.steerAngle = anguloGiro;
        ruedaDelanteraDerecha.steerAngle = anguloGiro;
    }

    void AplicarFuerzaSuelo()
    {
        // El Downforce empuja el coche hacia abajo en proporción a su velocidad.
        // Esto hace que se sienta "pesado" y con agarre en curvas rápidas, pero vuele si salta en una rampa.
        rb.AddForce(-transform.up * fuerzaDownforce * rb.linearVelocity.magnitude);
    }
}
